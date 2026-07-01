export type ApiResponse<T> = { ok: boolean; data?: T; status: number; error?: string; rawCode?: string }
