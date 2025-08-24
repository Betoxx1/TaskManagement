import React, { useState, useEffect } from 'react';
import { TaskFilters as TaskFiltersType, TaskStatus, TaskPriority } from '../types/task';

interface TaskFiltersProps {
  onFiltersChange: (filters: TaskFiltersType) => void;
  onClearFilters: () => void;
}

const TaskFilters: React.FC<TaskFiltersProps> = ({ onFiltersChange, onClearFilters }) => {
  const [filters, setFilters] = useState<TaskFiltersType>({});
  const [isExpanded, setIsExpanded] = useState(false);

  useEffect(() => {
    onFiltersChange(filters);
  }, [filters, onFiltersChange]);

  const handleFilterChange = (field: keyof TaskFiltersType, value: any) => {
    setFilters(prev => ({
      ...prev,
      [field]: value === '' ? undefined : value
    }));
  };

  const handleClearFilters = () => {
    setFilters({});
    onClearFilters();
  };

  const hasActiveFilters = Object.values(filters).some(value => 
    value !== undefined && value !== ''
  );

  return (
    <div className="task-filters">
      <div className="filters-header">
        <div className="filters-toggle">
          <button 
            className="toggle-btn"
            onClick={() => setIsExpanded(!isExpanded)}
          >
            🔍 Filtros {hasActiveFilters && <span className="active-indicator">({Object.values(filters).filter(v => v !== undefined && v !== '').length})</span>}
            <span className={`arrow ${isExpanded ? 'expanded' : ''}`}>▼</span>
          </button>
          {hasActiveFilters && (
            <button 
              className="clear-filters-btn"
              onClick={handleClearFilters}
            >
              Limpiar Filtros
            </button>
          )}
        </div>
      </div>

      {isExpanded && (
        <div className="filters-content">
          <div className="filters-row">
            <div className="filter-group">
              <label htmlFor="search">Buscar</label>
              <input
                type="text"
                id="search"
                value={filters.search || ''}
                onChange={(e) => handleFilterChange('search', e.target.value)}
                placeholder="Buscar en título y descripción..."
              />
            </div>

            <div className="filter-group">
              <label htmlFor="status">Estado</label>
              <select
                id="status"
                value={filters.status ?? ''}
                onChange={(e) => handleFilterChange('status', e.target.value === '' ? undefined : parseInt(e.target.value))}
              >
                <option value="">Todos los estados</option>
                <option value={TaskStatus.Pending}>Pendiente</option>
                <option value={TaskStatus.InProgress}>En Progreso</option>
                <option value={TaskStatus.Completed}>Completada</option>
                <option value={TaskStatus.Cancelled}>Cancelada</option>
              </select>
            </div>

            <div className="filter-group">
              <label htmlFor="priority">Prioridad</label>
              <select
                id="priority"
                value={filters.priority ?? ''}
                onChange={(e) => handleFilterChange('priority', e.target.value === '' ? undefined : parseInt(e.target.value))}
              >
                <option value="">Todas las prioridades</option>
                <option value={TaskPriority.Low}>Baja</option>
                <option value={TaskPriority.Medium}>Media</option>
                <option value={TaskPriority.High}>Alta</option>
                <option value={TaskPriority.Critical}>Crítica</option>
              </select>
            </div>

            <div className="filter-group">
              <label htmlFor="category">Categoría</label>
              <input
                type="text"
                id="category"
                value={filters.category || ''}
                onChange={(e) => handleFilterChange('category', e.target.value)}
                placeholder="Ej: Trabajo, Personal"
              />
            </div>
          </div>

          <div className="filters-row">
            <div className="filter-group">
              <label htmlFor="dueDateFrom">Vence desde</label>
              <input
                type="date"
                id="dueDateFrom"
                value={filters.dueDateFrom || ''}
                onChange={(e) => handleFilterChange('dueDateFrom', e.target.value)}
              />
            </div>

            <div className="filter-group">
              <label htmlFor="dueDateTo">Vence hasta</label>
              <input
                type="date"
                id="dueDateTo"
                value={filters.dueDateTo || ''}
                onChange={(e) => handleFilterChange('dueDateTo', e.target.value)}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TaskFilters;