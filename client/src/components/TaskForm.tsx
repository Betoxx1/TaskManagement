import React, { useState, useEffect } from 'react';
import { CreateTaskDto, UpdateTaskDto, Task, TaskStatus, TaskPriority } from '../types/task';

interface TaskFormProps {
  task?: Task; // Si se pasa, es edición; si no, es creación
  onSubmit: (taskData: CreateTaskDto | UpdateTaskDto) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

const TaskForm: React.FC<TaskFormProps> = ({ task, onSubmit, onCancel, isLoading = false }) => {
  const [formData, setFormData] = useState({
    title: task?.title || '',
    description: task?.description || '',
    status: task?.status ?? TaskStatus.Pending,
    priority: task?.priority ?? TaskPriority.Medium,
    dueDate: task?.dueDate ? task.dueDate.split('T')[0] : '', // Solo la fecha, sin la hora
    category: task?.category || '',
    tags: task?.tags || ''
  });

  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  const validateForm = (): boolean => {
    const newErrors: { [key: string]: string } = {};

    if (!formData.title.trim()) {
      newErrors.title = 'El título es requerido';
    } else if (formData.title.length > 200) {
      newErrors.title = 'El título no puede exceder 200 caracteres';
    }

    if (formData.description && formData.description.length > 1000) {
      newErrors.description = 'La descripción no puede exceder 1000 caracteres';
    }

    if (formData.category && formData.category.length > 100) {
      newErrors.category = 'La categoría no puede exceder 100 caracteres';
    }

    if (formData.tags && formData.tags.length > 500) {
      newErrors.tags = 'Las etiquetas no pueden exceder 500 caracteres';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      const submitData = {
        title: formData.title.trim(),
        description: formData.description.trim() || undefined,
        status: formData.status,
        priority: formData.priority,
        dueDate: formData.dueDate || undefined,
        category: formData.category.trim() || undefined,
        tags: formData.tags.trim() || undefined
      };

      await onSubmit(submitData);
    } catch (error) {
      console.error('Error submitting form:', error);
    }
  };

  const handleInputChange = (field: string, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Limpiar error del campo cuando el usuario empiece a escribir
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <div className="task-form-overlay">
      <div className="task-form-container">
        <div className="task-form-header">
          <h2>{task ? 'Editar Tarea' : 'Nueva Tarea'}</h2>
          <button className="close-btn" onClick={onCancel} disabled={isLoading}>
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="task-form">
          <div className="form-group">
            <label htmlFor="title">Título *</label>
            <input
              type="text"
              id="title"
              value={formData.title}
              onChange={(e) => handleInputChange('title', e.target.value)}
              className={errors.title ? 'error' : ''}
              disabled={isLoading}
              maxLength={200}
              placeholder="Ingresa el título de la tarea"
            />
            {errors.title && <span className="error-message">{errors.title}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="description">Descripción</label>
            <textarea
              id="description"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              className={errors.description ? 'error' : ''}
              disabled={isLoading}
              maxLength={1000}
              rows={4}
              placeholder="Describe la tarea (opcional)"
            />
            {errors.description && <span className="error-message">{errors.description}</span>}
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="status">Estado</label>
              <select
                id="status"
                value={formData.status}
                onChange={(e) => handleInputChange('status', parseInt(e.target.value))}
                disabled={isLoading}
              >
                <option value={TaskStatus.Pending}>Pendiente</option>
                <option value={TaskStatus.InProgress}>En Progreso</option>
                <option value={TaskStatus.Completed}>Completada</option>
                <option value={TaskStatus.Cancelled}>Cancelada</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="priority">Prioridad</label>
              <select
                id="priority"
                value={formData.priority}
                onChange={(e) => handleInputChange('priority', parseInt(e.target.value))}
                disabled={isLoading}
              >
                <option value={TaskPriority.Low}>Baja</option>
                <option value={TaskPriority.Medium}>Media</option>
                <option value={TaskPriority.High}>Alta</option>
                <option value={TaskPriority.Critical}>Crítica</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="dueDate">Fecha de Vencimiento</label>
              <input
                type="date"
                id="dueDate"
                value={formData.dueDate}
                onChange={(e) => handleInputChange('dueDate', e.target.value)}
                disabled={isLoading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="category">Categoría</label>
              <input
                type="text"
                id="category"
                value={formData.category}
                onChange={(e) => handleInputChange('category', e.target.value)}
                className={errors.category ? 'error' : ''}
                disabled={isLoading}
                maxLength={100}
                placeholder="Ej: Trabajo, Personal"
              />
              {errors.category && <span className="error-message">{errors.category}</span>}
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="tags">Etiquetas</label>
            <input
              type="text"
              id="tags"
              value={formData.tags}
              onChange={(e) => handleInputChange('tags', e.target.value)}
              className={errors.tags ? 'error' : ''}
              disabled={isLoading}
              maxLength={500}
              placeholder="Separadas por comas: urgente, reunión, cliente"
            />
            {errors.tags && <span className="error-message">{errors.tags}</span>}
            <small className="form-hint">Separa las etiquetas con comas</small>
          </div>

          <div className="form-actions">
            <button
              type="button"
              onClick={onCancel}
              className="btn btn-secondary"
              disabled={isLoading}
            >
              Cancelar
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={isLoading}
            >
              {isLoading ? 'Guardando...' : task ? 'Actualizar' : 'Crear Tarea'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default TaskForm;