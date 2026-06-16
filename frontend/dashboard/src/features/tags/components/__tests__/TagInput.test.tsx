import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor, fireEvent } from '@testing-library/react'
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

  it('does not update tag list when getTags returns ok=false', async () => {
    mockGetTags.mockResolvedValue({ ok: false })
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await waitFor(() => expect(mockGetTags).toHaveBeenCalledOnce())
    await user.click(screen.getByRole('combobox'))
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
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
    await waitFor(() => expect(screen.getByRole('menuitem', { name: 'food' })).toBeInTheDocument())
    await user.click(screen.getByRole('menuitem', { name: 'food' }))
    expect(onChange).toHaveBeenCalledWith([ownTag])
  })

  it('selecting family tag calls onChange with tag in array', async () => {
    setupGetTags(makeList({ family: [familyTag] }))
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('menuitem', { name: 'travel' })).toBeInTheDocument())
    await user.click(screen.getByRole('menuitem', { name: 'travel' }))
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

  it('does not call onChange when useTag returns ok=false', async () => {
    setupGetTags(emptyList)
    mockUseTag.mockResolvedValue({ ok: false })
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.type(screen.getByRole('combobox'), 'xyz')
    await waitFor(() => expect(screen.getByText(/Create/)).toBeInTheDocument())
    await user.click(screen.getByText(/Create/))
    await waitFor(() => expect(mockUseTag).toHaveBeenCalledWith('xyz'))
    expect(onChange).not.toHaveBeenCalled()
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
    await waitFor(() => expect(screen.getByRole('menu')).toBeInTheDocument())
    await user.keyboard('{Escape}')
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
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

  it('pressing Enter selects first result', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('menuitem', { name: 'food' })).toBeInTheDocument())
    await user.keyboard('{Enter}')
    expect(onChange).toHaveBeenCalledWith([ownTag])
  })

  it('pressing Enter triggers create when no results match', async () => {
    setupGetTags(emptyList)
    const newTag: Tag = { id: 3, name: 'xyz' }
    mockUseTag.mockResolvedValue({ ok: true, data: newTag })
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.type(screen.getByRole('combobox'), 'xyz')
    await waitFor(() => expect(screen.getByText(/Create/)).toBeInTheDocument())
    await user.keyboard('{Enter}')
    await waitFor(() => expect(mockUseTag).toHaveBeenCalledWith('xyz'))
    expect(onChange).toHaveBeenCalledWith([newTag])
  })

  it('dropdown closes after selection', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('menuitem', { name: 'food' })).toBeInTheDocument())
    await user.click(screen.getByRole('menuitem', { name: 'food' }))
    expect(screen.queryByRole('menu')).not.toBeInTheDocument()
  })

  it('closes dropdown when clicking outside the component', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('menu')).toBeInTheDocument())
    fireEvent.mouseDown(document.body)
    await waitFor(() => expect(screen.queryByRole('menu')).not.toBeInTheDocument())
  })

  it('shows placeholder hint when no tags selected', () => {
    setupGetTags()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    expect(screen.getByRole('combobox')).toHaveAttribute('placeholder', expect.stringMatching(/enter/i))
  })

  it('hides placeholder once a tag chip exists', () => {
    setupGetTags()
    render(<TagInput value={[ownTag]} onChange={vi.fn()} />)
    expect(screen.getByRole('combobox')).not.toHaveAttribute('placeholder')
  })

  it('does not show + button when input is empty', () => {
    setupGetTags()
    render(<TagInput value={[]} onChange={vi.fn()} />)
    expect(screen.queryByRole('button', { name: 'Add tag' })).not.toBeInTheDocument()
  })

  it('pressing comma selects first result same as Enter', async () => {
    setupGetTags(makeList({ own: [ownTag] }))
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.click(screen.getByRole('combobox'))
    await waitFor(() => expect(screen.getByRole('menuitem', { name: 'food' })).toBeInTheDocument())
    await user.keyboard(',')
    expect(onChange).toHaveBeenCalledWith([ownTag])
  })

  it('pressing comma triggers create when no results match', async () => {
    setupGetTags(emptyList)
    const newTag: Tag = { id: 3, name: 'xyz' }
    mockUseTag.mockResolvedValue({ ok: true, data: newTag })
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.type(screen.getByRole('combobox'), 'xyz')
    await waitFor(() => expect(screen.getByText(/Create/)).toBeInTheDocument())
    await user.keyboard(',')
    await waitFor(() => expect(mockUseTag).toHaveBeenCalledWith('xyz'))
    expect(onChange).toHaveBeenCalledWith([newTag])
  })

  it('clicking + button creates tag from current query', async () => {
    setupGetTags(emptyList)
    const newTag: Tag = { id: 3, name: 'new' }
    mockUseTag.mockResolvedValue({ ok: true, data: newTag })
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<TagInput value={[]} onChange={onChange} />)
    await user.type(screen.getByRole('combobox'), 'new')
    await waitFor(() => expect(screen.getByRole('button', { name: 'Add tag' })).toBeInTheDocument())
    await user.click(screen.getByRole('button', { name: 'Add tag' }))
    await waitFor(() => expect(mockUseTag).toHaveBeenCalledWith('new'))
    expect(onChange).toHaveBeenCalledWith([newTag])
  })
})
