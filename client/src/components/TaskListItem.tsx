import React, { useState } from 'react';
import { Edit2, Trash2, Calendar, Folder, Tag, Clock, CheckCircle2, Circle, AlertTriangle, ChevronDown, ChevronRight } from 'lucide-react';
import { Task, TaskStatus, TaskPriority } from '../types/task';

interface TaskListItemProps {
  task: Task;
  onEdit: (task: Task) => void;
  onDelete: (taskId: number) => void;
  onStatusChange: (taskId: number, newStatus: TaskStatus) => void;
  onTagClick?: (tag: string) => void;
  isSelected?: boolean;
  onSelect?: (taskId: number, selected: boolean) => void;
}

const TaskListItem: React.FC<TaskListItemProps> = ({
  task,
  onEdit,
  onDelete,
  onStatusChange,
  onTagClick,
  isSelected = false,
  onSelect
}) => {
  const [showDescription, setShowDescription] = useState(false);

  const formatDate = (dateString?: string): string => {
    if (!dateString) return '';
    return new Date(dateString).toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  };

  const isOverdue = (dueDateString?: string): boolean => {
    if (!dueDateString) return false;
    const dueDate = new Date(dueDateString);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return dueDate < today && task.status !== TaskStatus.Completed;
  };

  const getTagsList = (tags: string | null | undefined): string[] => {
    // Verificar que tags sea un string válido antes de hacer split
    if (!tags || typeof tags !== 'string') {
      console.log('⚠️ [TaskListItem] Tags no es un string válido:', tags, typeof tags);
      return [];
    }
    
    try {
      return tags.split(',').map(tag => tag.trim()).filter(tag => tag);
    } catch (error) {
      console.error('❌ [TaskListItem] Error procesando tags:', error, tags);
      return [];
    }
  };

  const getPriorityConfig = (priority: TaskPriority) => {
    switch (priority) {
      case TaskPriority.Low:
        return { color: '#10b981', label: 'Baja' };
      case TaskPriority.Medium:
        return { color: '#f59e0b', label: 'Media' };
      case TaskPriority.High:
        return { color: '#f97316', label: 'Alta' };
      case TaskPriority.Critical:
        return { color: '#ef4444', label: 'Crítica' };
      default:
        return { color: '#6b7280', label: 'Sin prioridad' };
    }
  };

  const getStatusIcon = (status: TaskStatus) => {
    switch (status) {
      case TaskStatus.Completed:
        return <CheckCircle2 size={20} className="status-checkbox completed" />;
      case TaskStatus.InProgress:
        return <Clock size={20} className="status-checkbox in-progress" />;
      case TaskStatus.Cancelled:
        return <Circle size={20} className="status-checkbox cancelled" />;
      default:
        return <Circle size={20} className="status-checkbox pending" />;
    }
  };

  const handleQuickStatusToggle = () => {
    if (task.status === TaskStatus.Completed) {
      onStatusChange(task.id, TaskStatus.Pending);
    } else {
      onStatusChange(task.id, TaskStatus.Completed);
    }
  };

  const handleStatusChange = (newStatus: string) => {
    onStatusChange(task.id, parseInt(newStatus) as TaskStatus);
  };

  const handleTagClick = (tag: string) => {
    if (onTagClick) {
      onTagClick(tag);
    }
  };

  const handleSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (onSelect) {
      onSelect(task.id, e.target.checked);
    }
  };

  const priorityConfig = getPriorityConfig(task.priority);
  const tags = getTagsList(task.tags);
  const isTaskOverdue = isOverdue(task.dueDate);

  return (
    <div className={`task-list-item ${isSelected ? 'selected' : ''} ${task.status === TaskStatus.Completed ? 'completed' : ''}`}>
      {/* Primera fila: Status dropdown y Acciones (Arriba) */}
      <div className="task-header-row">
        {/* Selection checkbox (if multiple selection is enabled) */}
        {onSelect && (
          <div className="task-select">
            <input
              type="checkbox"
              checked={isSelected}
              onChange={handleSelect}
              className="select-checkbox"
            />
          </div>
        )}

        {/* Status dropdown */}
        <div className="task-status-dropdown">
          <select
            value={task.status}
            onChange={(e) => handleStatusChange(e.target.value)}
            className="status-dropdown"
            onClick={(e) => e.stopPropagation()}
          >
            <option value={TaskStatus.Pending}>Pendiente</option>
            <option value={TaskStatus.InProgress}>En Progreso</option>
            <option value={TaskStatus.Completed}>Completada</option>
            <option value={TaskStatus.Cancelled}>Cancelada</option>
          </select>
        </div>

        {/* Actions */}
        <div className="task-actions">
          <button 
            className="action-btn edit-btn"
            onClick={(e) => {
              e.stopPropagation();
              onEdit(task);
            }}
            title="Editar tarea"
          >
            <Edit2 size={14} />
          </button>
          <button 
            className="action-btn delete-btn"
            onClick={(e) => {
              e.stopPropagation();
              onDelete(task.id);
            }}
            title="Eliminar tarea"
          >
            <Trash2 size={14} />
          </button>
        </div>
      </div>

      {/* Segunda fila: Status checkbox, Title, Priority Badge y Tags */}
      <div className="task-title-row">
        {/* Status checkbox */}
        <div className="task-status" onClick={handleQuickStatusToggle}>
          {getStatusIcon(task.status)}
        </div>

        <h3 className={`task-title ${task.status === TaskStatus.Completed ? 'completed' : ''}`}>
          {task.title}
        </h3>
        
        <div className="task-meta-inline">
          <div 
            className="priority-badge"
            style={{ backgroundColor: priorityConfig.color }}
          >
            {priorityConfig.label}
          </div>

          {/* Tags junto al priority badge */}
          {tags.length > 0 && (
            <div className="task-tags-inline">
              {tags.slice(0, 3).map((tag, index) => (
                <span 
                  key={index} 
                  className="tag-chip"
                  onClick={() => handleTagClick(tag)}
                >
                  {tag}
                </span>
              ))}
              {tags.length > 3 && (
                <span className="tag-chip more-tags">+{tags.length - 3}</span>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Descripción oculta por defecto */}
      {task.description && (
        <div className="task-description-section">
          <button 
            className="description-toggle-btn"
            onClick={() => setShowDescription(!showDescription)}
          >
            {showDescription ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
            <span>Descripción</span>
          </button>
          
          {showDescription && (
            <div className="task-description">
              <p className="description-text">{task.description}</p>
            </div>
          )}
        </div>
      )}

      {/* Meta información */}
      <div className="task-meta-info">
        {/* Due date */}
        {task.dueDate && (
          <div className={`meta-item due-date ${isTaskOverdue ? 'overdue' : ''}`}>
            <Calendar size={12} />
            <span>{formatDate(task.dueDate)}</span>
            {isTaskOverdue && <AlertTriangle size={12} className="overdue-icon" />}
          </div>
        )}

        {/* Category */}
        {task.category && (
          <div className="meta-item category">
            <Folder size={12} />
            <span>{task.category}</span>
          </div>
        )}

        {/* Creation/update dates */}
        <div className="meta-item dates">
          <span className="created-date">Creada: {formatDate(task.createdAt)}</span>
          {task.updatedAt && task.updatedAt !== task.createdAt && (
            <span className="updated-date">• Actualizada: {formatDate(task.updatedAt)}</span>
          )}
        </div>
      </div>
    </div>
  );
};

export default TaskListItem;