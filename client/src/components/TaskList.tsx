import React, { useState, useMemo } from 'react';
import { ChevronDown, ChevronUp, CheckSquare, Square } from 'lucide-react';
import { Task, TaskStatus, TaskPriority } from '../types/task';
import TaskListItem from './TaskListItem';

interface TaskListProps {
  tasks: Task[];
  onEdit: (task: Task) => void;
  onDelete: (taskId: number) => void;
  onStatusChange: (taskId: number, newStatus: TaskStatus) => void;
  onTagClick?: (tag: string) => void;
  loading?: boolean;
}

type SortOption = 'dueDate' | 'priority' | 'status' | 'title' | 'created';
type SortDirection = 'asc' | 'desc';

const TaskList: React.FC<TaskListProps> = ({
  tasks,
  onEdit,
  onDelete,
  onStatusChange,
  onTagClick,
  loading = false
}) => {
  const [selectedTasks, setSelectedTasks] = useState<Set<number>>(new Set());
  const [sortBy, setSortBy] = useState<SortOption>('dueDate');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [showCompleted, setShowCompleted] = useState(true);
  const [bulkActionsOpen, setBulkActionsOpen] = useState(false);

  // Sort tasks based on current sort criteria
  const sortedTasks = useMemo(() => {
    const filtered = showCompleted ? tasks : tasks.filter(t => t.status !== TaskStatus.Completed);
    
    return [...filtered].sort((a, b) => {
      let aValue, bValue;
      
      switch (sortBy) {
        case 'dueDate':
          aValue = a.dueDate ? new Date(a.dueDate).getTime() : Infinity;
          bValue = b.dueDate ? new Date(b.dueDate).getTime() : Infinity;
          break;
        case 'priority':
          aValue = a.priority;
          bValue = b.priority;
          break;
        case 'status':
          aValue = a.status;
          bValue = b.status;
          break;
        case 'title':
          aValue = a.title.toLowerCase();
          bValue = b.title.toLowerCase();
          break;
        case 'created':
          aValue = new Date(a.createdAt).getTime();
          bValue = new Date(b.createdAt).getTime();
          break;
        default:
          return 0;
      }
      
      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
      return 0;
    });
  }, [tasks, sortBy, sortDirection, showCompleted]);

  // Group tasks by status for better organization
  const groupedTasks = useMemo(() => {
    const groups = {
      pending: sortedTasks.filter(t => t.status === TaskStatus.Pending),
      inProgress: sortedTasks.filter(t => t.status === TaskStatus.InProgress),
      completed: sortedTasks.filter(t => t.status === TaskStatus.Completed),
      cancelled: sortedTasks.filter(t => t.status === TaskStatus.Cancelled),
    };
    return groups;
  }, [sortedTasks]);

  const handleSort = (option: SortOption) => {
    if (sortBy === option) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(option);
      setSortDirection('asc');
    }
  };

  const handleSelectTask = (taskId: number, selected: boolean) => {
    const newSelected = new Set(selectedTasks);
    if (selected) {
      newSelected.add(taskId);
    } else {
      newSelected.delete(taskId);
    }
    setSelectedTasks(newSelected);
  };

  const handleSelectAll = () => {
    if (selectedTasks.size === sortedTasks.length) {
      setSelectedTasks(new Set());
    } else {
      setSelectedTasks(new Set(sortedTasks.map(t => t.id)));
    }
  };

  const handleBulkStatusChange = (newStatus: TaskStatus) => {
    selectedTasks.forEach(taskId => {
      onStatusChange(taskId, newStatus);
    });
    setSelectedTasks(new Set());
    setBulkActionsOpen(false);
  };

  const handleBulkDelete = () => {
    if (window.confirm(`¿Estás seguro de que quieres eliminar ${selectedTasks.size} tareas?`)) {
      selectedTasks.forEach(taskId => {
        onDelete(taskId);
      });
      setSelectedTasks(new Set());
      setBulkActionsOpen(false);
    }
  };

  const getSortIcon = (option: SortOption) => {
    if (sortBy !== option) return null;
    return sortDirection === 'asc' ? <ChevronUp size={14} /> : <ChevronDown size={14} />;
  };

  const renderTaskGroup = (tasks: Task[], title: string, count: number) => {
    if (tasks.length === 0) return null;
    
    return (
      <div className="task-group">
        <div className="task-group-header">
          <h4 className="task-group-title">
            {title} <span className="task-count">({count})</span>
          </h4>
        </div>
        <div className="task-group-content">
          {tasks.map(task => (
            <TaskListItem
              key={task.id}
              task={task}
              onEdit={onEdit}
              onDelete={onDelete}
              onStatusChange={onStatusChange}
              onTagClick={onTagClick}
              isSelected={selectedTasks.has(task.id)}
              onSelect={selectedTasks.size > 0 || bulkActionsOpen ? handleSelectTask : undefined}
            />
          ))}
        </div>
      </div>
    );
  };

  if (loading) {
    return (
      <div className="task-list-loading">
        <div className="loading-spinner"></div>
        <p>Cargando tareas...</p>
      </div>
    );
  }

  if (tasks.length === 0) {
    return (
      <div className="task-list-empty">
        <div className="empty-icon">📋</div>
        <h3>No hay tareas</h3>
        <p>¡Comienza creando tu primera tarea!</p>
      </div>
    );
  }

  return (
    <div className="task-list-container">
      {/* Controls bar */}
      <div className="task-list-controls">
        <div className="sort-controls">
          <span className="control-label">Ordenar por:</span>
          <button 
            className={`sort-btn ${sortBy === 'dueDate' ? 'active' : ''}`}
            onClick={() => handleSort('dueDate')}
          >
            Fecha {getSortIcon('dueDate')}
          </button>
          <button 
            className={`sort-btn ${sortBy === 'priority' ? 'active' : ''}`}
            onClick={() => handleSort('priority')}
          >
            Prioridad {getSortIcon('priority')}
          </button>
          <button 
            className={`sort-btn ${sortBy === 'status' ? 'active' : ''}`}
            onClick={() => handleSort('status')}
          >
            Estado {getSortIcon('status')}
          </button>
          <button 
            className={`sort-btn ${sortBy === 'title' ? 'active' : ''}`}
            onClick={() => handleSort('title')}
          >
            Título {getSortIcon('title')}
          </button>
        </div>

        <div className="view-controls">
          <button 
            className={`view-btn ${showCompleted ? 'active' : ''}`}
            onClick={() => setShowCompleted(!showCompleted)}
          >
            {showCompleted ? 'Ocultar completadas' : 'Mostrar completadas'}
          </button>
          
          <button 
            className={`bulk-btn ${bulkActionsOpen ? 'active' : ''}`}
            onClick={() => setBulkActionsOpen(!bulkActionsOpen)}
          >
            Selección múltiple
          </button>
        </div>
      </div>

      {/* Bulk actions bar */}
      {(selectedTasks.size > 0 || bulkActionsOpen) && (
        <div className="bulk-actions-bar">
          <div className="bulk-selection">
            <button 
              className="select-all-btn"
              onClick={handleSelectAll}
            >
              {selectedTasks.size === sortedTasks.length ? <CheckSquare size={16} /> : <Square size={16} />}
              {selectedTasks.size === sortedTasks.length ? 'Deseleccionar todas' : 'Seleccionar todas'}
            </button>
            {selectedTasks.size > 0 && (
              <span className="selection-count">{selectedTasks.size} tareas seleccionadas</span>
            )}
          </div>
          
          {selectedTasks.size > 0 && (
            <div className="bulk-actions">
              <button 
                className="bulk-action-btn status-pending"
                onClick={() => handleBulkStatusChange(TaskStatus.Pending)}
              >
                Marcar Pendientes
              </button>
              <button 
                className="bulk-action-btn status-progress"
                onClick={() => handleBulkStatusChange(TaskStatus.InProgress)}
              >
                Marcar En Progreso
              </button>
              <button 
                className="bulk-action-btn status-completed"
                onClick={() => handleBulkStatusChange(TaskStatus.Completed)}
              >
                Marcar Completadas
              </button>
              <button 
                className="bulk-action-btn delete"
                onClick={handleBulkDelete}
              >
                Eliminar
              </button>
            </div>
          )}
        </div>
      )}

      {/* Task groups */}
      <div className="task-list-content">
        {renderTaskGroup(groupedTasks.pending, 'Pendientes', groupedTasks.pending.length)}
        {renderTaskGroup(groupedTasks.inProgress, 'En Progreso', groupedTasks.inProgress.length)}
        {showCompleted && renderTaskGroup(groupedTasks.completed, 'Completadas', groupedTasks.completed.length)}
        {renderTaskGroup(groupedTasks.cancelled, 'Canceladas', groupedTasks.cancelled.length)}
      </div>
    </div>
  );
};

export default TaskList;