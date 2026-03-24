export interface ApiError {
  message: string;
  code?: string;
  errors?: Record<string, string[]>;
}

export function parseApiError(error: unknown): ApiError {
  const httpError = error as { error?: Partial<ApiError>; status?: number; message?: string };

  // Сервер вернул ApiError
  if (httpError?.error?.message) {
    return {
      message: httpError.error.message,
      code: httpError.error.code ?? undefined,
      errors: httpError.error.errors ?? undefined,
    };
  }

  // Нет соединения с сервером
  if (httpError?.status === 0) {
    return { message: 'Нет соединения с сервером.', code: 'NETWORK_ERROR' };
  }

  // Стандартные HTTP-ошибки без тела
  const statusMessages: Record<number, string> = {
    401: 'Сессия истекла. Войдите заново.',
    403: 'Доступ запрещён.',
    404: 'Ресурс не найден.',
    500: 'Ошибка сервера. Попробуйте позже.',
  };

  if (httpError?.status && statusMessages[httpError.status]) {
    return { message: statusMessages[httpError.status], code: `HTTP_${httpError.status}` };
  }

  return { message: 'Произошла непредвиденная ошибка.', code: 'UNKNOWN_ERROR' };
}
