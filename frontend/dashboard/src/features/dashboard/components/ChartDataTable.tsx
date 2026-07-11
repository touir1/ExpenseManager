export type ChartDataTableColumn = { key: string; header: string }
export type ChartDataTableRow = Record<string, string | number>

type Props = {
  caption: string
  columns: ChartDataTableColumn[]
  rows: ChartDataTableRow[]
}

/**
 * Visually-hidden data table mirroring a chart's series — Recharts SVGs expose
 * nothing to screen readers, so this is the only way that audience gets the values.
 */
export function ChartDataTable({ caption, columns, rows }: Props) {
  return (
    <table className="sr-only">
      <caption>{caption}</caption>
      <thead>
        <tr>
          {columns.map(c => (
            <th key={c.key} scope="col">{c.header}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.map((row, i) => (
          <tr key={i}>
            {columns.map(c => (
              <td key={c.key}>{row[c.key]}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  )
}
