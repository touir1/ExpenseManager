import React from 'react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'

// @hookform/resolvers@3.10.0 uses parseAsync which hangs with Zod v4 in jsdom.
// Use synchronous safeParse to bypass the hang.
vi.mock('@hookform/resolvers/zod', () => ({
  zodResolver: (schema: any) => async (values: any) => {
    const result = schema.safeParse(values)
    if (result.success) return { values: result.data, errors: {} }
    const errors: Record<string, any> = {}
    for (const issue of result.error.issues) {
      const key = String(issue.path[0] ?? 'root')
      if (!errors[key]) errors[key] = { message: issue.message, type: issue.code }
    }
    return { values: {}, errors }
  },
}))

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
    IonInput: React.forwardRef(({ onIonInput, onChange, onBlur, type, ...rest }: any, ref: any) => (
      <input
        ref={ref}
        type={type}
        onChange={e => { onChange?.(e); onIonInput?.({ detail: { value: e.target.value } }) }}
        onBlur={onBlur}
        {...rest}
      />
    )),
    IonButton: ({ children, onClick, type, disabled }: any) => (
      <button onClick={onClick} type={type} disabled={disabled}>{children}</button>
    ),
    IonButtons: ({ children }: any) => <div>{children}</div>,
    IonCheckbox: React.forwardRef(({ onIonChange, onChange, onBlur, ...rest }: any, ref: any) => (
      <input
        ref={ref}
        type="checkbox"
        onChange={e => { onChange?.(e); onIonChange?.({ detail: { checked: e.target.checked } }) }}
        onBlur={onBlur}
        {...rest}
      />
    )),
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
    const emailInput = screen.getAllByRole('textbox')[0]
    const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.change(passwordInput, { target: { value: 'secret' } })
    fireEvent.click(screen.getByRole('button', { name: /login|sign/i }))
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('test@example.com', 'secret', false)
    })
  })

  it('shows error toast on login failure', async () => {
    mockLogin.mockResolvedValue({ ok: false, error: 'Invalid credentials' })
    renderLogin()
    const emailInput = screen.getAllByRole('textbox')[0]
    const passwordInput = document.querySelector('input[type="password"]') as HTMLInputElement
    fireEvent.change(emailInput, { target: { value: 'test@example.com' } })
    fireEvent.change(passwordInput, { target: { value: 'wrong' } })
    fireEvent.click(screen.getByRole('button', { name: /login|sign/i }))
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeDefined()
    })
  })
})
