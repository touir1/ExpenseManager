const PALETTE = [
  { bg: '#C8623E26', text: '#C8623E' }, // burnt orange
  { bg: '#6B8E5A26', text: '#6B8E5A' }, // sage green
  { bg: '#D6A23F26', text: '#8B6720' }, // golden
  { bg: '#5C8C9E26', text: '#5C8C9E' }, // steel blue
  { bg: '#B5443F26', text: '#B5443F' }, // deep red
  { bg: '#E8B89A40', text: '#A0602E' }, // peach
  { bg: '#7A5C9E26', text: '#7A5C9E' }, // soft purple
  { bg: '#3D8A8A26', text: '#3D8A8A' }, // teal
  { bg: '#7A8B3526', text: '#7A8B35' }, // olive
  { bg: '#9E5C6E26', text: '#9E5C6E' }, // dusty rose
  { bg: '#4E8B7A26', text: '#4E8B7A' }, // seafoam
  { bg: '#9E7A3526', text: '#9E7A35' }, // amber
  { bg: '#3D705826', text: '#3D7058' }, // forest green
  { bg: '#8E5C7A26', text: '#8E5C7A' }, // mauve pink
  { bg: '#5C6E9E26', text: '#5C6E9E' }, // medium indigo
  { bg: '#6E4A8E26', text: '#6E4A8E' }, // deep purple
  { bg: '#5C7A8E26', text: '#5C7A8E' }, // slate
]

export function getCategoryColor(categoryId: number | undefined): { bg: string; text: string } {
  if (categoryId == null) return { bg: '#64748B1A', text: '#64748B' }
  return PALETTE[categoryId % PALETTE.length]
}
