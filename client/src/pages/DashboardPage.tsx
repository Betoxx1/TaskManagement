import React, { useState, useEffect, useCallback } from 'react';
import { Plus, RotateCcw, User, LogOut, ClipboardList, Clock, Rocket, CheckCircle2, AlertCircle, X, ClipboardCheck } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';
import { Task, CreateTaskDto, UpdateTaskDto, TaskFilters as TaskFiltersType, TaskStatus } from '../types/task';
import taskService from '../services/taskService';
import TaskList from '../components/TaskList';
import TaskForm from '../components/TaskForm';
import TaskFilters from '../components/TaskFilters';
import './DashboardPage.css';
import '../components/TaskList.css';
import '../components/TaskListItem.css';

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
      console.log('🚀 [DashboardPage] Iniciando carga de tareas...');
      setLoading(true);
      setError(null);
      
      const tasksData = await taskService.getAllTasks();
      
      console.log('📥 [DashboardPage] Tareas recibidas del servicio:', tasksData);
      console.log('🔢 [DashboardPage] Cantidad de tareas:', tasksData?.length || 0);
      console.log('📝 [DashboardPage] Primera tarea (si existe):', tasksData?.[0]);
      
      setTasks(tasksData);
      setFilteredTasks(tasksData);
      
      console.log('✅ [DashboardPage] Estado actualizado con tareas');
    } catch (err) {
      console.error('❌ [DashboardPage] Error loading tasks:', err);
      setError(err instanceof Error ? err.message : 'Error cargando tareas');
    } finally {
      setLoading(false);
      console.log('🏁 [DashboardPage] Carga de tareas finalizada');
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
        task.description.toLowerCase().includes(searchLower) ||
        task.tags.toLowerCase().includes(searchLower)
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

  // Manejar filtro por tag
  const handleTagFilter = useCallback((tag: string) => {
    const newFilters: TaskFiltersType = { search: tag };
    handleFiltersChange(newFilters);
  }, [handleFiltersChange]);

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
    logout();
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
          <h1>
            <ClipboardCheck size={28} style={{ marginRight: '8px', display: 'inline-block' }} />
            Task Management
          </h1>
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
            <div className="stat-icon">
              <ClipboardList size={32} />
            </div>
            <div className="stat-info">
              <span className="stat-number">{stats.total}</span>
              <span className="stat-label">Total</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">
              <Clock size={32} />
            </div>
            <div className="stat-info">
              <span className="stat-number">{stats.pending}</span>
              <span className="stat-label">Pendientes</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">
              <Rocket size={32} />
            </div>
            <div className="stat-info">
              <span className="stat-number">{stats.inProgress}</span>
              <span className="stat-label">En Progreso</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">
              <CheckCircle2 size={32} />
            </div>
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
            <Plus size={16} />
            Nueva Tarea
          </button>
          <button 
            className="btn btn-secondary"
            onClick={loadTasks}
            disabled={loading}
          >
            <RotateCcw size={16} className={loading ? 'rotating' : ''} />
            {loading ? 'Cargando...' : 'Actualizar'}
          </button>
        </div>

        {/* Error Message */}
        {error && (
          <div className="error-banner">
            <span>
              <AlertCircle size={16} style={{ marginRight: '8px', display: 'inline-block' }} />
              {error}
            </span>
            <button onClick={() => setError(null)}>
              <X size={16} />
            </button>
          </div>
        )}

        {/* Filters */}
        <TaskFilters 
          onFiltersChange={handleFiltersChange}
          onClearFilters={handleClearFilters}
        />

        {/* Tasks List */}
        <div className="tasks-section">
          <TaskList
            tasks={filteredTasks}
            onEdit={handleEditTask}
            onDelete={handleDeleteTask}
            onStatusChange={handleStatusChange}
            onTagClick={handleTagFilter}
            loading={loading}
          />
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