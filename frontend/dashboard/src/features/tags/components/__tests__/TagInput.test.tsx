import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import TagInput from '../TagInput'
import type { Tag, TagList } from '../../types/tag.type'

const mockGetTags = vi.fn()
const mockUseTag = vi.fn()

vi.mock('@/features/tags/services/tagsApi.service', () => ({
  getTags: () => mockGetTags(),
  useTag: (name: string) => mockUseTag(name),
  removeTag: vi.fn(),
}))

const ownTag: Tag = { id: 1, name: 'food' }
const familyTag: Tag = { id: 2, name: 'travel' }
const emptyList: TagList = { own: [], family: [] }

function makeList(overrides: Partial<TagList> = {}): TagList {
  return { own: [], family: [], ...overrides }
}

function setupGetTags(list: TagList = emptyList) {
  mockGetTags.mockResolvedValue({ ok: true, data: list })
}

describe('TagInput', () => {
  beforeEach(() => vi.clearAllMocks())

  it('renders text input with no chips initially', () => {
    setupGetTags()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    expect(screen.getByRole('combobox')).toBeInTheDocument()
  })

  it('fetches tags on mount', async () => {
    setupGetTags()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await waitFor(() => expect(mockGetTags).toHaveBeenCalledOnce())
  })

  it('shows My tags group heading when own tags match', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByText('My tags')).toBeInTheDocument())
  })

  it('shows Family tags group heading when family tags match', async () => {
    setupGetTags(makeList({ family: [familyTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByText('Family tags')).toBeInTheDocument())
  })

  it('shows create option when typed name has no exact match', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.type(screen.getByRole('combobox'), 'xyz')
    await waitFor(() => expect(screen.getByText(/Create/)).toBeInTheDocument())
  })

  it('does not show create option when exact name match exists in own tags', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.type(screen.getByRole('combobox'), 'food')
    await waitFor(() => expect(screen.queryByText(/Create/)).not.toBeInTheDocument())
  })

  it('selecting own tag calls onChange with tag in array', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('option', { name: 'food' })).toBeInTheDocument())
    await user.click(screen.getByRole('option', { name: 'food' }))
    expect(onChange).toHaveBeenCalledWith([ownTag])
  })

  it('selecting family tag calls onChange with tag in array', async () => {
    setupGetTags(makeList({ family: [familyTag] }))
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('option', { name: 'travel' })).toBeInTheDocument())
    await user.click(screen.getByRole('option', { name: 'travel' }))
    expect(onChange).toHaveBeenCalledWith([familyTag])
  })

  it('creating tag calls useTag then onChange', async () => {
    setupGetTags(emptyList)
    const newTag: Tag = { id: 3, name: 'new' }
    mockUseTag.mockResolvedValue({ ok: true, data: newTag })
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.type(screen.getByRole('combobox'), 'new')
    await waitFor(() => expect(screen.getByText(/Create/)).toBeInTheDocument())
    await user.click(screen.getByText(/Create/))
    await waitFor(() => expect(mockUseTag).toHaveBeenCalledWith('new'))
    expect(onChange).toHaveBeenCalledWith([newTag])
  })

  it('clicking chip remove button removes tag from onChange', async () => {
    setupGetTags(emptyList)
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[ownTag]} onChange={onChange} />)
    await user.click(screen.getByRole('button', { name: 'Remove food' }))
    expect(onChange).toHaveBeenCalledWith([])
  })

  it('pressing Escape closes dropdown', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('listbox')).toBeInTheDocument())
    await user.keyboard('{Escape}')
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })

  it('pressing Backspace removes last chip', async () => {
    setupGetTags(emptyList)
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[ownTag, familyTag]} onChange={onChange} />)
    await user.click(screen.getByRole('combobox'))
    await user.keyboard('{Backspace}')
    expect(onChange).toHaveBeenCalledWith([ownTag])
  })

  it('dropdown closes after selection', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('option', { name: 'food' })).toBeInTheDocument())
    await user.click(screen.getByRole('option', { name: 'food' }))
    expect(screen.queryByRole('listbox')).not.toBeInTheDocument()
  })
})
