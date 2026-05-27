const PALETTE = [
  { bg: '#C8623E26', text: '#C8623E' },
  { bg: '#6B8E5A26', text: '#6B8E5A' },
  { bg: '#D6A23F26', text: '#8B6720' },
  { bg: '#5C8C9E26', text: '#5C8C9E' },
  { bg: '#B5443F26', text: '#B5443F' },
  { bg: '#E8B89A40', text: '#A0602E' },
]

export const CHART_COLORS = PALETTE.map(p => p.text)

export function getCategoryColor(categoryId: number | undefined): { bg: string; text: string } {
  if (categoryId == null) return { bg: '#64748B1A', text: '#64748B' }
  return PALETTE[categoryId % PALETTE.length]
}
