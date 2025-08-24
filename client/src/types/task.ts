export enum TaskStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface Task {
  id: number;
  title: string;
  description: string;
  status: TaskStatus;
  priority: TaskPriority;
  createdAt: string;
  updatedAt?: string;
  dueDate?: string;
  userId: string;
  category: string;
  tags: string;
}

export interface CreateTaskDto {
  title: string;
  description?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  dueDate?: string;
  category?: string;
  tags?: string;
}

export interface UpdateTaskDto {
  title?: string;
  description?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  dueDate?: string;
  category?: string;
  tags?: string;
}

export interface TaskApiResponse<T = any> {
  success: boolean;
  data: T;
  message: string;
}

export interface TaskFilters {
  status?: TaskStatus;
  priority?: TaskPriority;
  category?: string;
  search?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
}

// Helper functions para mostrar valores legibles
export const getStatusLabel = (status: TaskStatus): string => {
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
      return 'Desconocido';
  }
};

export const getPriorityLabel = (priority: TaskPriority): string => {
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
      return 'Desconocida';
  }
};

export const getStatusColor = (status: TaskStatus): string => {
  switch (status) {
    case TaskStatus.Pending:
      return '#6b7280'; // gray
    case TaskStatus.InProgress:
      return '#3b82f6'; // blue
    case TaskStatus.Completed:
      return '#10b981'; // green
    case TaskStatus.Cancelled:
      return '#ef4444'; // red
    default:
      return '#6b7280';
  }
};

export const getPriorityColor = (priority: TaskPriority): string => {
  switch (priority) {
    case TaskPriority.Low:
      return '#10b981'; // green
    case TaskPriority.Medium:
      return '#f59e0b'; // yellow
    case TaskPriority.High:
      return '#f97316'; // orange
    case TaskPriority.Critical:
      return '#ef4444'; // red
    default:
      return '#6b7280';
  }
};