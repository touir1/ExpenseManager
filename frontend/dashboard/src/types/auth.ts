export type User = { email: string; firstName?: string }

export type AuthContextValue = {
  isAuthenticated: boolean
  isLoading: boolean
  user: User | null
  login: (email: string, password: string, rememberMe?: boolean) => Promise<boolean>
  logout: () => void
  register: (firstName: string, lastName: string, email: string) => Promise<boolean>
  changePassword: (oldPassword: string, newPassword: string, repeatPassword: string) => Promise<boolean>
  resetPassword: (email: string, verificationHash: string, newPassword: string, repeatPassword: string) => Promise<boolean>
  requestPasswordReset?: (email: string) => Promise<boolean>
}
