import React, { useState, useEffect } from 'react';
import { Filter, ChevronDown, Search, X, Calendar, Flag, Clock, Rocket, CheckCircle2, XCircle } from 'lucide-react';
import { TaskFilters as TaskFiltersType, TaskStatus, TaskPriority } from '../types/task';
import './TaskFilters.css';

interface TaskFiltersProps {
  onFiltersChange: (filters: TaskFiltersType) => void;
  onClearFilters: () => void;
}

const TaskFilters: React.FC<TaskFiltersProps> = ({ onFiltersChange, onClearFilters }) => {
  const [filters, setFilters] = useState<TaskFiltersType>({});
  const [isExpanded, setIsExpanded] = useState(false);

  // Helper functions para iconos
  const getStatusIcon = (status?: TaskStatus) => {
    switch (status) {
      case TaskStatus.Pending:
        return <Clock size={14} className="status-icon pending" />;
      case TaskStatus.InProgress:
        return <Rocket size={14} className="status-icon in-progress" />;
      case TaskStatus.Completed:
        return <CheckCircle2 size={14} className="status-icon completed" />;
      case TaskStatus.Cancelled:
        return <XCircle size={14} className="status-icon cancelled" />;
      default:
        return null;
    }
  };

  const getPriorityIcon = (priority?: TaskPriority) => {
    const baseClass = "priority-icon";
    switch (priority) {
      case TaskPriority.Low:
        return <Flag size={14} className={`${baseClass} low`} />;
      case TaskPriority.Medium:
        return <Flag size={14} className={`${baseClass} medium`} />;
      case TaskPriority.High:
        return <Flag size={14} className={`${baseClass} high`} />;
      case TaskPriority.Critical:
        return <Flag size={14} className={`${baseClass} critical`} />;
      default:
        return <Flag size={14} className={baseClass} />;
    }
  };

  const getStatusLabel = (status?: TaskStatus) => {
    switch (status) {
      case TaskStatus.Pending:
        return 'Pendiente';
      case TaskStatus.InProgress:
        return 'En Progreso';
      case TaskStatus.Completed:
        return 'Completada';
      case TaskStatus.Cancelled:
        return 'Cancelada';
      default:
        return 'Todos';
    }
  };

  const getPriorityLabel = (priority?: TaskPriority) => {
    switch (priority) {
      case TaskPriority.Low:
        return 'Baja';
      case TaskPriority.Medium:
        return 'Media';
      case TaskPriority.High:
        return 'Alta';
      case TaskPriority.Critical:
        return 'Crítica';
      default:
        return 'Todas';
    }
  };

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
            <Filter size={16} />
            Filtros {hasActiveFilters && <span className="active-indicator">({Object.values(filters).filter(v => v !== undefined && v !== '').length})</span>}
            <ChevronDown size={16} className={`arrow ${isExpanded ? 'expanded' : ''}`} />
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
          <div className="filters-grid">
            {/* Search Filter */}
            <div className="filter-group search-group">
              <label htmlFor="search">
                <Search size={14} />
                Buscar
              </label>
              <div className="input-wrapper">
                <input
                  type="text"
                  id="search"
                  value={filters.search || ''}
                  onChange={(e) => handleFilterChange('search', e.target.value)}
                  placeholder="Buscar tareas..."
                />
                {filters.search && (
                  <button 
                    className="clear-input-btn"
                    onClick={() => handleFilterChange('search', '')}
                    type="button"
                  >
                    <X size={14} />
                  </button>
                )}
              </div>
            </div>

            {/* Status Filter */}
            <div className="filter-group">
              <label htmlFor="status">Estado</label>
              <div className="custom-select-wrapper">
                <div className="select-display">
                  {getStatusIcon(filters.status)}
                  <span>{getStatusLabel(filters.status)}</span>
                </div>
                <select
                  id="status"
                  value={filters.status ?? ''}
                  onChange={(e) => handleFilterChange('status', e.target.value === '' ? undefined : parseInt(e.target.value))}
                >
                  <option value="">Todos</option>
                  <option value={TaskStatus.Pending}>Pendiente</option>
                  <option value={TaskStatus.InProgress}>En Progreso</option>
                  <option value={TaskStatus.Completed}>Completada</option>
                  <option value={TaskStatus.Cancelled}>Cancelada</option>
                </select>
              </div>
            </div>

            {/* Priority Filter */}
            <div className="filter-group">
              <label htmlFor="priority">
                <Flag size={14} />
                Prioridad
              </label>
              <div className="custom-select-wrapper">
                <div className="select-display">
                  {getPriorityIcon(filters.priority)}
                  <span>{getPriorityLabel(filters.priority)}</span>
                </div>
                <select
                  id="priority"
                  value={filters.priority ?? ''}
                  onChange={(e) => handleFilterChange('priority', e.target.value === '' ? undefined : parseInt(e.target.value))}
                >
                  <option value="">Todas</option>
                  <option value={TaskPriority.Low}>Baja</option>
                  <option value={TaskPriority.Medium}>Media</option>
                  <option value={TaskPriority.High}>Alta</option>
                  <option value={TaskPriority.Critical}>Crítica</option>
                </select>
              </div>
            </div>

            {/* Category Filter */}
            <div className="filter-group">
              <label htmlFor="category">Categoría</label>
              <div className="input-wrapper">
                <input
                  type="text"
                  id="category"
                  value={filters.category || ''}
                  onChange={(e) => handleFilterChange('category', e.target.value)}
                  placeholder="Ej: Trabajo, Personal"
                />
                {filters.category && (
                  <button 
                    className="clear-input-btn"
                    onClick={() => handleFilterChange('category', '')}
                    type="button"
                  >
                    <X size={14} />
                  </button>
                )}
              </div>
            </div>

            {/* Date Range Filters */}
            <div className="filter-group date-group">
              <label htmlFor="dueDateFrom">
                <Calendar size={14} />
                Fecha vencimiento
              </label>
              <div className="date-range">
                <input
                  type="date"
                  id="dueDateFrom"
                  value={filters.dueDateFrom || ''}
                  onChange={(e) => handleFilterChange('dueDateFrom', e.target.value)}
                  title="Desde"
                />
                <span className="date-separator">-</span>
                <input
                  type="date"
                  id="dueDateTo"
                  value={filters.dueDateTo || ''}
                  onChange={(e) => handleFilterChange('dueDateTo', e.target.value)}
                  title="Hasta"
                />
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TaskFilters;