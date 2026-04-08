import axios from "axios";

const AUTH_STORAGE_KEY = "taskflow.auth";

const api = axios.create({
  baseURL: "/api",
});

export function getStoredAuth() {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

export function setStoredAuth(auth) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
}

export function clearStoredAuth() {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

api.interceptors.request.use((config) => {
  const auth = getStoredAuth();

  if (auth?.token) {
    config.headers.Authorization = `Bearer ${auth.token}`;
  }

  return config;
});

export const authApi = {
  login: (data) => api.post("/auth/login", data),
  register: (data) => api.post("/auth/register", data),
  me: () => api.get("/auth/me"),
};

export const taskApi = {
  getAllCompany: () => api.get("/task/company/all"),
  getManagerInbox: () => api.get("/task/manager/inbox"),
  getCreated: (managerId) => api.get(`/task/created/${managerId}`),
  getAssigned: (employeeId) => api.get(`/task/assigned/${employeeId}`),
  getById: (id) => api.get(`/task/${id}`),
  create: (data) => api.post("/task", data),
  createCustomerRequest: (data) => api.post("/task/customer-request", data),
  assignToEmployee: (id, employeeUserId) => api.patch(`/task/${id}/assign`, { employeeUserId }),
  returnToCustomer: (id) => api.patch(`/task/${id}/return-to-customer`),
  complete: (id) => api.patch(`/task/${id}/complete`),
  updateStatus: (id, status) => api.patch(`/task/${id}/status`, JSON.stringify(status), {
    headers: { "Content-Type": "application/json" },
  }),
  delete: (id) => api.delete(`/task/${id}`),
};

export const userApi = {
  getMe: () => api.get("/user/me"),
  updateMe: (data) => api.put("/user/me", data),
  changeMyPassword: (data) => api.patch("/user/me/password", data),
  deleteMe: () => api.delete("/user/me"),
  getCompanyEmployees: () => api.get("/user/company"),
  getManagers: () => api.get("/user/managers"),
};

export const companyApi = {
  getAll: () => api.get("/company"),
  getPublic: () => api.get("/company/public"),
  create: (data) => api.post("/company", data),
};

export const fileApi = {
  getByTask: (taskId) => api.get(`/tasks/${taskId}/files`),
  upload: (taskId, file, category) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("category", category);

    return api.post(`/tasks/${taskId}/files`, formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
  download: (id) =>
    api.get(`/files/${id}/download`, {
      responseType: "blob",
    }),
};

export function extractErrorMessage(error, fallback = "Something went wrong.") {
  return error?.response?.data?.message || error?.response?.data || fallback;
}

export default api;
