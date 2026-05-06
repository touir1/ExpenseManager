export type Subcategory = {
  id: number
  name: string
  description?: string
}

export type Category = {
  id: number
  name: string
  description?: string
  subcategories: Subcategory[]
}

export type Currency = {
  id: number
  code: string
  name: string
  symbol: string
  decimals: number
}
