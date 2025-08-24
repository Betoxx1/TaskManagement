import axios, { AxiosResponse } from 'axios';
import { Task, CreateTaskDto, UpdateTaskDto, TaskApiResponse, TaskFilters } from '../types/task';

class TaskService {
  private readonly baseURL: string;

  constructor() {
    this.baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:7071/api';
  }

  private getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  // Obtener todas las tareas
  async getAllTasks(): Promise<Task[]> {
    try {
      const response: AxiosResponse<TaskApiResponse<Task[]>> = await axios.get(
        `${this.baseURL}/task`,
        { headers: this.getAuthHeaders() }
      );
      
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message || 'Error obteniendo tareas');
    } catch (error) {
      console.error('Error fetching tasks:', error);
      throw this.handleError(error);
    }
  }

  // Obtener tarea por ID
  async getTaskById(id: number): Promise<Task> {
    try {
      const response: AxiosResponse<TaskApiResponse<Task>> = await axios.get(
        `${this.baseURL}/task/${id}`,
        { headers: this.getAuthHeaders() }
      );
      
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message || 'Error obteniendo tarea');
    } catch (error) {
      console.error('Error fetching task:', error);
      throw this.handleError(error);
    }
  }

  // Crear nueva tarea
  async createTask(taskData: CreateTaskDto): Promise<Task> {
    try {
      const response: AxiosResponse<TaskApiResponse<Task>> = await axios.post(
        `${this.baseURL}/task/create`,
        taskData,
        { headers: this.getAuthHeaders() }
      );
      
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message || 'Error creando tarea');
    } catch (error) {
      console.error('Error creating task:', error);
      throw this.handleError(error);
    }
  }

  // Actualizar tarea
  async updateTask(id: number, taskData: UpdateTaskDto): Promise<Task> {
    try {
      const response: AxiosResponse<TaskApiResponse<Task>> = await axios.put(
        `${this.baseURL}/task/${id}`,
        taskData,
        { headers: this.getAuthHeaders() }
      );
      
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message || 'Error actualizando tarea');
    } catch (error) {
      console.error('Error updating task:', error);
      throw this.handleError(error);
    }
  }

  // Eliminar tarea
  async deleteTask(id: number): Promise<void> {
    try {
      const response: AxiosResponse<TaskApiResponse> = await axios.delete(
        `${this.baseURL}/task/${id}`,
        { headers: this.getAuthHeaders() }
      );
      
      if (!response.data.success) {
        throw new Error(response.data.message || 'Error eliminando tarea');
      }
    } catch (error) {
      console.error('Error deleting task:', error);
      throw this.handleError(error);
    }
  }

  // Filtrar tareas
  async filterTasks(filters: TaskFilters): Promise<Task[]> {
    try {
      const queryParams = new URLSearchParams();
      
      if (filters.status !== undefined) queryParams.append('status', filters.status.toString());
      if (filters.priority !== undefined) queryParams.append('priority', filters.priority.toString());
      if (filters.category) queryParams.append('category', filters.category);
      if (filters.search) queryParams.append('search', filters.search);
      if (filters.dueDateFrom) queryParams.append('dueDateFrom', filters.dueDateFrom);
      if (filters.dueDateTo) queryParams.append('dueDateTo', filters.dueDateTo);

      const response: AxiosResponse<TaskApiResponse<Task[]>> = await axios.get(
        `${this.baseURL}/task/filter?${queryParams.toString()}`,
        { headers: this.getAuthHeaders() }
      );
      
      if (response.data.success) {
        return response.data.data;
      }
      throw new Error(response.data.message || 'Error filtrando tareas');
    } catch (error) {
      console.error('Error filtering tasks:', error);
      throw this.handleError(error);
    }
  }

  // Manejo de errores
  private handleError(error: any): Error {
    if (error.response) {
      // El servidor respondió con un status de error
      const status = error.response.status;
      const data = error.response.data;
      
      if (status === 401) {
        // Token inválido o expirado
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
        return new Error('Sesión expirada. Por favor, inicia sesión nuevamente.');
      }
      
      if (status === 403) {
        return new Error('No tienes permisos para realizar esta acción.');
      }
      
      if (status === 404) {
        return new Error('Tarea no encontrada.');
      }
      
      if (data && data.error) {
        return new Error(data.error);
      }
      
      return new Error(`Error del servidor (${status})`);
    }
    
    if (error.request) {
      // La request fue hecha pero no se recibió respuesta
      return new Error('No se pudo conectar con el servidor. Verifica tu conexión.');
    }
    
    // Error en la configuración de la request
    return new Error(error.message || 'Error desconocido');
  }
}

export default new TaskService();