﻿namespace WebAPI.DTOs.Admin_DTO
{
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }

}
