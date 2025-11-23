// src/api.js
import axios from "axios";

// Aapke UserAPI ka sahi URL
const api = axios.create({
    baseURL: "https://localhost:7107/api",
});

export default api;
