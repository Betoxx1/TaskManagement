import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { Task, CreateTaskDto, UpdateTaskDto, TaskFilters as TaskFiltersType, TaskStatus } from '../types/task';
import taskService from '../services/taskService';
import TaskCard from '../components/TaskCard';
import TaskForm from '../components/TaskForm';
import TaskFilters from '../components/TaskFilters';
import './DashboardPage.css';
import '../components/TaskComponents.css';

const DashboardPage: React.FC = () => {
  const { user, logout } = useAuth();
  
  // Estado
  const [tasks, setTasks] = useState<Task[]>([]);
  const [filteredTasks, setFilteredTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showTaskForm, setShowTaskForm] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [formLoading, setFormLoading] = useState(false);

  // Cargar tareas
  const loadTasks = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const tasksData = await taskService.getAllTasks();
      setTasks(tasksData);
      setFilteredTasks(tasksData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error cargando tareas');
      console.error('Error loading tasks:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  // Manejar filtros
  const handleFiltersChange = useCallback((filters: TaskFiltersType) => {
    let filtered = [...tasks];

    if (filters.search) {
      const searchLower = filters.search.toLowerCase();
      filtered = filtered.filter(task =>
        task.title.toLowerCase().includes(searchLower) ||
        task.description.toLowerCase().includes(searchLower)
      );
    }

    if (filters.status !== undefined) {
      filtered = filtered.filter(task => task.status === filters.status);
    }

    if (filters.priority !== undefined) {
      filtered = filtered.filter(task => task.priority === filters.priority);
    }

    if (filters.category) {
      filtered = filtered.filter(task =>
        task.category.toLowerCase().includes(filters.category!.toLowerCase())
      );
    }

    if (filters.dueDateFrom) {
      filtered = filtered.filter(task =>
        task.dueDate && task.dueDate >= filters.dueDateFrom!
      );
    }

    if (filters.dueDateTo) {
      filtered = filtered.filter(task =>
        task.dueDate && task.dueDate <= filters.dueDateTo!
      );
    }

    setFilteredTasks(filtered);
  }, [tasks]);

  const handleClearFilters = useCallback(() => {
    setFilteredTasks(tasks);
  }, [tasks]);

  // CRUD Operations
  const handleCreateTask = async (taskData: CreateTaskDto) => {
    try {
      setFormLoading(true);
      const newTask = await taskService.createTask(taskData);
      setTasks(prev => [newTask, ...prev]);
      setFilteredTasks(prev => [newTask, ...prev]);
      setShowTaskForm(false);
    } catch (err) {
      throw err; // Let the form handle the error
    } finally {
      setFormLoading(false);
    }
  };

  const handleUpdateTask = async (taskData: UpdateTaskDto) => {
    if (!editingTask) return;
    
    try {
      setFormLoading(true);
      const updatedTask = await taskService.updateTask(editingTask.id, taskData);
      
      setTasks(prev => prev.map(task =>
        task.id === editingTask.id ? updatedTask : task
      ));
      setFilteredTasks(prev => prev.map(task =>
        task.id === editingTask.id ? updatedTask : task
      ));
      
      setEditingTask(null);
      setShowTaskForm(false);
    } catch (err) {
      throw err; // Let the form handle the error
    } finally {
      setFormLoading(false);
    }
  };

  const handleDeleteTask = async (taskId: number) => {
    if (!window.confirm('¿Estás seguro de que quieres eliminar esta tarea?')) {
      return;
    }

    try {
      await taskService.deleteTask(taskId);
      setTasks(prev => prev.filter(task => task.id !== taskId));
      setFilteredTasks(prev => prev.filter(task => task.id !== taskId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error eliminando tarea');
    }
  };

  const handleStatusChange = async (taskId: number, newStatus: TaskStatus) => {
    try {
      const updatedTask = await taskService.updateTask(taskId, { status: newStatus });
      
      setTasks(prev => prev.map(task =>
        task.id === taskId ? updatedTask : task
      ));
      setFilteredTasks(prev => prev.map(task =>
        task.id === taskId ? updatedTask : task
      ));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error actualizando estado');
    }
  };

  const handleEditTask = (task: Task) => {
    setEditingTask(task);
    setShowTaskForm(true);
  };

  const handleCloseForm = () => {
    setShowTaskForm(false);
    setEditingTask(null);
  };

  const handleLogout = () => {
    if (window.confirm('¿Estás seguro de que quieres cerrar sesión?')) {
      logout();
    }
  };

  // Estadísticas
  const stats = {
    total: tasks.length,
    pending: tasks.filter(t => t.status === TaskStatus.Pending).length,
    inProgress: tasks.filter(t => t.status === TaskStatus.InProgress).length,
    completed: tasks.filter(t => t.status === TaskStatus.Completed).length,
  };

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <div className="header-content">
          <h1>📋 Task Management</h1>
          <div className="user-menu">
            <div className="user-info">
              <span className="user-name">{user?.name}</span>
              <span className="user-email">{user?.email}</span>
            </div>
            <button className="logout-button" onClick={handleLogout}>
              Cerrar Sesión
            </button>
          </div>
        </div>
      </header>

      <main className="dashboard-main">
        {/* Stats Cards */}
        <div className="stats-section">
          <div className="stat-card">
            <div className="stat-icon">📝</div>
            <div className="stat-info">
              <span className="stat-number">{stats.total}</span>
              <span className="stat-label">Total</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">⏳</div>
            <div className="stat-info">
              <span className="stat-number">{stats.pending}</span>
              <span className="stat-label">Pendientes</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">🚀</div>
            <div className="stat-info">
              <span className="stat-number">{stats.inProgress}</span>
              <span className="stat-label">En Progreso</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">✅</div>
            <div className="stat-info">
              <span className="stat-number">{stats.completed}</span>
              <span className="stat-label">Completadas</span>
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="actions-bar">
          <button 
            className="btn btn-primary"
            onClick={() => setShowTaskForm(true)}
          >
            ➕ Nueva Tarea
          </button>
          <button 
            className="btn btn-secondary"
            onClick={loadTasks}
            disabled={loading}
          >
            {loading ? '🔄 Cargando...' : '🔄 Actualizar'}
          </button>
        </div>

        {/* Error Message */}
        {error && (
          <div className="error-banner">
            <span>❌ {error}</span>
            <button onClick={() => setError(null)}>✕</button>
          </div>
        )}

        {/* Filters */}
        <TaskFilters 
          onFiltersChange={handleFiltersChange}
          onClearFilters={handleClearFilters}
        />

        {/* Tasks List */}
        <div className="tasks-section">
          {loading && (
            <div className="loading-message">
              <div className="loading-spinner"></div>
              <p>Cargando tareas...</p>
            </div>
          )}

          {!loading && filteredTasks.length === 0 && (
            <div className="empty-state">
              <div className="empty-icon">📝</div>
              <h3>No hay tareas</h3>
              <p>
                {tasks.length === 0 
                  ? '¡Comienza creando tu primera tarea!'
                  : 'No se encontraron tareas con los filtros actuales.'
                }
              </p>
            </div>
          )}

          <div className="tasks-grid">
            {filteredTasks.map(task => (
              <TaskCard
                key={task.id}
                task={task}
                onEdit={handleEditTask}
                onDelete={handleDeleteTask}
                onStatusChange={handleStatusChange}
              />
            ))}
          </div>
        </div>
      </main>

      {/* Task Form Modal */}
      {showTaskForm && (
        <TaskForm
          task={editingTask || undefined}
          onSubmit={editingTask ? handleUpdateTask : handleCreateTask}
          onCancel={handleCloseForm}
          isLoading={formLoading}
        />
      )}
    </div>
  );
};

export default DashboardPage; 