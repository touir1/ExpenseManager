import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import AuthBrandPanel from '../AuthBrandPanel'

describe('AuthBrandPanel', () => {
  it('renders the brand wordmark', () => {
    render(<AuthBrandPanel />)
    expect(screen.getByText(/ExpenseManager/)).toBeInTheDocument()
  })

  it('renders the tagline heading', () => {
    render(<AuthBrandPanel />)
    expect(screen.getByRole('heading', { level: 2 })).toBeInTheDocument()
  })

  it('renders the floating receipt card example', () => {
    render(<AuthBrandPanel />)
    expect(screen.getByText('Weekly shop')).toBeInTheDocument()
  })

  it('renders the footer copyright text', () => {
    render(<AuthBrandPanel />)
    expect(screen.getByText(/ExpenseManager, 2026/)).toBeInTheDocument()
  })
})
