import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'

vi.mock('@/features/auth/AuthContext', () => ({
  useAuth: vi.fn(),
  AuthProvider: ({ children }: any) => <>{children}</>,
}))

vi.mock('@ionic/react', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@ionic/react')>()
  return {
    ...actual,
    IonPage: ({ children }: any) => <div>{children}</div>,
    IonContent: ({ children }: any) => <div>{children}</div>,
    IonHeader: ({ children }: any) => <div>{children}</div>,
    IonToolbar: ({ children }: any) => <div>{children}</div>,
    IonTitle: ({ children }: any) => <h1>{children}</h1>,
    IonItem: ({ children }: any) => <div>{children}</div>,
    IonLabel: ({ children }: any) => <label>{children}</label>,
    IonInput: ({ onIonInput, ...props }: any) => (
      <input onChange={e => onIonInput?.({ detail: { value: e.target.value } })} {...props} />
    ),
    IonButton: ({ children, onClick, type, disabled }: any) => (
      <button onClick={onClick} type={type} disabled={disabled}>{children}</button>
    ),
    IonButtons: ({ children }: any) => <div>{children}</div>,
    IonCheckbox: ({ onIonChange, ...props }: any) => (
      <input type="checkbox" onChange={e => onIonChange?.({ detail: { checked: e.target.checked } })} {...props} />
    ),
    IonText: ({ children, color }: any) => <span data-color={color}>{children}</span>,
    IonSpinner: () => <span>Loading</span>,
    IonToast: ({ isOpen, message }: any) => isOpen ? <div role="alert">{message}</div> : null,
  }
})

import { useAuth } from '@/features/auth/AuthContext'
import LoginPage from '@/features/auth/pages/LoginPage'

const mockLogin = vi.fn()
const useAuthMock = useAuth as ReturnType<typeof vi.fn>

function renderLogin() {
  return render(
    <MemoryRouter>
      <LoginPage />
    </MemoryRouter>
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    useAuthMock.mockReturnValue({
      login: mockLogin,
      isAuthenticated: false,
      isLoading: false,
      user: null,
    })
    mockLogin.mockReset()
  })

  it('renders email and password fields', () => {
    renderLogin()
    expect(screen.getByText('Expenses Manager')).toBeDefined()
  })

  it('shows validation errors when submitting empty form', async () => {
    renderLogin()
    const submitBtn = screen.getByRole('button', { name: /login|sign/i })
    fireEvent.click(submitBtn)
    await waitFor(() => {
      expect(mockLogin).not.toHaveBeenCalled()
    })
  })

  it('calls auth.login with credentials on valid submit', async () => {
    mockLogin.mockResolvedValue({ ok: true })
    renderLogin()
    const inputs = screen.getAllByRole('textbox')
    // fill email
    fireEvent.change(inputs[0], { target: { value: 'test@example.com' } })
    const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement
    fireEvent.change(passwordInput, { target: { value: 'secret' } })
    const submitBtn = screen.getByRole('button', { name: /login|sign/i })
    fireEvent.click(submitBtn)
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('test@example.com', 'secret', false)
    })
  })

  it('shows error toast on login failure', async () => {
    mockLogin.mockResolvedValue({ ok: false, error: 'Invalid credentials' })
    renderLogin()
    const inputs = screen.getAllByRole('textbox')
    fireEvent.change(inputs[0], { target: { value: 'test@example.com' } })
    const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement
    fireEvent.change(passwordInput, { target: { value: 'wrong' } })
    fireEvent.click(screen.getByRole('button', { name: /login|sign/i }))
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeDefined()
    })
  })
})
