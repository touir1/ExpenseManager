export type User = { email: string; firstName?: string; lastName?: string }

export type AuthResult = { ok: boolean; error?: string }

export type AuthContextValue = {
  isAuthenticated: boolean
  isLoading: boolean
  user: User | null
  login: (email: string, password: string, rememberMe?: boolean) => Promise<AuthResult>
  logout: () => void
  register: (firstName: string, lastName: string, email: string) => Promise<AuthResult>
  changePassword: (oldPassword: string, newPassword: string, repeatPassword: string) => Promise<AuthResult>
  resetPassword: (email: string, verificationHash: string, newPassword: string, repeatPassword: string) => Promise<AuthResult>
  requestPasswordReset?: (email: string) => Promise<AuthResult>
}
