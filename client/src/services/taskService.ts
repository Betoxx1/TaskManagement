import axios, { AxiosResponse } from 'axios';
import { Task, CreateTaskDto, UpdateTaskDto, TaskApiResponse, TaskFilters } from '../types/task';

class TaskService {
  private readonly baseURL: string;

  constructor() {
    this.baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:7071/api';
  }

  // Transformar datos del backend (PascalCase) a frontend (camelCase)
  private transformTaskFromBackend(backendTask: any): Task {
    console.log('🔄 [TaskService] Transformando tarea del backend:', backendTask);
    console.log('🏷️ [TaskService] Tags del backend - Valor:', backendTask.Tags, 'Tipo:', typeof backendTask.Tags);
    
    const transformed = {
      id: backendTask.Id,
      title: backendTask.Title,
      description: backendTask.Description || '',
      status: backendTask.Status,
      priority: backendTask.Priority || 1, // Default Medium
      createdAt: backendTask.CreatedAt,
      updatedAt: backendTask.UpdatedAt,
      dueDate: backendTask.DueDate,
      userId: backendTask.UserId,
      category: backendTask.Category || '',
      tags: backendTask.Tags ? String(backendTask.Tags) : '' // Asegurar que sea string
    };
    
    console.log('✅ [TaskService] Tarea transformada:', transformed);
    console.log('🏷️ [TaskService] Tags transformados - Valor:', transformed.tags, 'Tipo:', typeof transformed.tags);
    return transformed;
  }

  private getAuthHeaders() {
    const token = localStorage.getItem('token');
    console.log('🔑 [TaskService] Token from localStorage:', token ? `${token.substring(0, 20)}...` : 'NULL');
    console.log('🔑 [TaskService] Token exists:', !!token);
    
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  // Obtener todas las tareas
  async getAllTasks(): Promise<Task[]> {
    try {
      console.log('🔄 [TaskService] Iniciando llamada a getAllTasks...');
      console.log('🔗 [TaskService] URL:', `${this.baseURL}/task`);
      console.log('🔑 [TaskService] Headers:', this.getAuthHeaders());
      
      const response: AxiosResponse<TaskApiResponse<Task[]>> = await axios.get(
        `${this.baseURL}/task`,
        { headers: this.getAuthHeaders() }
      );
      
      console.log('📨 [TaskService] Respuesta completa del backend:', response);
      console.log('📊 [TaskService] Status HTTP:', response.status);
      console.log('📋 [TaskService] Data recibida:', response.data);
      console.log('✅ [TaskService] Success:', response.data.success);
      console.log('📝 [TaskService] Message:', response.data.message);
      console.log('🎯 [TaskService] Tasks data:', response.data.data);
      console.log('🔢 [TaskService] Número de tareas:', response.data.data?.length || 0);
      
      if (response.data.success) {
        console.log('✅ [TaskService] Transformando tareas del backend...');
        const transformedTasks = response.data.data.map((task: any) => this.transformTaskFromBackend(task));
        console.log('🎯 [TaskService] Tareas transformadas:', transformedTasks);
        console.log('✅ [TaskService] Retornando tareas exitosamente');
        return transformedTasks;
      }
      throw new Error(response.data.message || 'Error obteniendo tareas');
    } catch (error) {
      console.error('❌ [TaskService] Error fetching tasks:', error);
      if (error.response) {
        console.error('❌ [TaskService] Error response status:', error.response.status);
        console.error('❌ [TaskService] Error response data:', error.response.data);
        console.error('❌ [TaskService] Error response headers:', error.response.headers);
      }
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
        return this.transformTaskFromBackend(response.data.data);
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
        return this.transformTaskFromBackend(response.data.data);
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
        return this.transformTaskFromBackend(response.data.data);
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
        return response.data.data.map((task: any) => this.transformTaskFromBackend(task));
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