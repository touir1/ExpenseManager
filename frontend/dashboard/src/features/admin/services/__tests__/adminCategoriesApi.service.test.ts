import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import * as api from '@/services/api.service'
import {
  getCategories,
  addCategory,
  updateCategory,
  archiveCategory,
  unarchiveCategory,
  addSubcategory,
  updateSubcategory,
  archiveSubcategory,
  unarchiveSubcategory,
} from '../adminCategoriesApi.service'

vi.mock('@/services/api.service', () => ({
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
}))

const ok = { ok: true, status: 200, data: {} }
const BASE = '/api/expenses/admin/categories'

beforeEach(() => vi.clearAllMocks())
afterEach(() => vi.clearAllMocks())

describe('getCategories', () => {
  it('calls GET base', async () => {
    vi.mocked(api.get).mockResolvedValue(ok)
    await getCategories()
    expect(api.get).toHaveBeenCalledWith(BASE)
  })
})

describe('addCategory', () => {
  it('calls POST base with name only', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addCategory('Food')
    expect(api.post).toHaveBeenCalledWith(BASE, { name: 'Food', description: undefined })
  })

  it('calls POST base with name and description', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addCategory('Food', 'All food expenses')
    expect(api.post).toHaveBeenCalledWith(BASE, { name: 'Food', description: 'All food expenses' })
  })
})

describe('updateCategory', () => {
  it('calls PUT {id} with fields', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await updateCategory(1, 'Groceries', 'desc')
    expect(api.put).toHaveBeenCalledWith(`${BASE}/1`, { name: 'Groceries', description: 'desc' })
  })
})

describe('archiveCategory', () => {
  it('calls POST {id}/archive', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await archiveCategory(2)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/2/archive`, {})
  })
})

describe('unarchiveCategory', () => {
  it('calls POST {id}/unarchive', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await unarchiveCategory(2)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/2/unarchive`, {})
  })
})

describe('addSubcategory', () => {
  it('calls POST {catId}/subcategories with fields', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await addSubcategory(1, 'Fruits')
    expect(api.post).toHaveBeenCalledWith(`${BASE}/1/subcategories`, { name: 'Fruits', description: undefined })
  })
})

describe('updateSubcategory', () => {
  it('calls PUT {catId}/subcategories/{subId} with fields', async () => {
    vi.mocked(api.put).mockResolvedValue(ok)
    await updateSubcategory(1, 5, 'Vegetables', 'Fresh veg')
    expect(api.put).toHaveBeenCalledWith(`${BASE}/1/subcategories/5`, { name: 'Vegetables', description: 'Fresh veg' })
  })
})

describe('archiveSubcategory', () => {
  it('calls POST {catId}/subcategories/{subId}/archive', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await archiveSubcategory(1, 5)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/1/subcategories/5/archive`, {})
  })
})

describe('unarchiveSubcategory', () => {
  it('calls POST {catId}/subcategories/{subId}/unarchive', async () => {
    vi.mocked(api.post).mockResolvedValue(ok)
    await unarchiveSubcategory(1, 5)
    expect(api.post).toHaveBeenCalledWith(`${BASE}/1/subcategories/5/unarchive`, {})
  })
})
