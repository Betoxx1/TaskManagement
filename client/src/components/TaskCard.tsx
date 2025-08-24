import React from 'react';
import { Edit2, Trash2, Calendar, Folder, Tag, Circle, CheckCircle2, Clock, XCircle } from 'lucide-react';
import { Task, getStatusLabel, getPriorityLabel, getStatusColor, getPriorityColor } from '../types/task';

interface TaskCardProps {
  task: Task;
  onEdit: (task: Task) => void;
  onDelete: (taskId: number) => void;
  onStatusChange: (taskId: number, newStatus: number) => void;
}

const TaskCard: React.FC<TaskCardProps> = ({ task, onEdit, onDelete, onStatusChange }) => {
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
    return dueDate < today && task.status !== 2; // Not completed
  };

  const getTagsList = (tags: string): string[] => {
    return tags ? tags.split(',').map(tag => tag.trim()).filter(tag => tag) : [];
  };

  return (
    <div className="task-card">
      <div className="task-card-header">
        <h3 className="task-title">{task.title}</h3>
        <div className="task-actions">
          <button 
            className="action-btn edit-btn"
            onClick={() => onEdit(task)}
            title="Editar tarea"
          >
            <Edit2 size={16} />
          </button>
          <button 
            className="action-btn delete-btn"
            onClick={() => onDelete(task.id)}
            title="Eliminar tarea"
          >
            <Trash2 size={16} />
          </button>
        </div>
      </div>

      {task.description && (
        <p className="task-description">{task.description}</p>
      )}

      <div className="task-meta">
        <div className="task-status-priority">
          <span 
            className="status-badge"
            style={{ backgroundColor: getStatusColor(task.status) }}
          >
            {getStatusLabel(task.status)}
          </span>
          <span 
            className="priority-badge"
            style={{ backgroundColor: getPriorityColor(task.priority) }}
          >
            {getPriorityLabel(task.priority)}
          </span>
        </div>

        {task.category && (
          <div className="task-category">
            <Folder size={14} />
            <span>{task.category}</span>
          </div>
        )}

        {task.dueDate && (
          <div className={`task-due-date ${isOverdue(task.dueDate) ? 'overdue' : ''}`}>
            <Calendar size={14} />
            <span>Vence: {formatDate(task.dueDate)}</span>
            {isOverdue(task.dueDate) && <span className="overdue-indicator">¡Vencida!</span>}
          </div>
        )}

        {getTagsList(task.tags).length > 0 && (
          <div className="task-tags">
            <Tag size={14} />
            <div className="tags-list">
              {getTagsList(task.tags).map((tag, index) => (
                <span key={index} className="tag">
                  {tag}
                </span>
              ))}
            </div>
          </div>
        )}

        <div className="task-dates">
          <small>Creada: {formatDate(task.createdAt)}</small>
          {task.updatedAt && (
            <small>Actualizada: {formatDate(task.updatedAt)}</small>
          )}
        </div>
      </div>

      <div className="task-status-changer">
        <select
          value={task.status}
          onChange={(e) => onStatusChange(task.id, parseInt(e.target.value))}
          className="status-select"
        >
          <option value={0}>Pendiente</option>
          <option value={1}>En Progreso</option>
          <option value={2}>Completada</option>
          <option value={3}>Cancelada</option>
        </select>
      </div>
    </div>
  );
};

export default TaskCard;