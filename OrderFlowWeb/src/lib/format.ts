const currencyFormatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

const timeFormatter = new Intl.DateTimeFormat('pt-BR', { timeStyle: 'medium' })

const secondsFormatter = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 1 })

export const formatCurrency = (value: number) => currencyFormatter.format(value)

export const formatTime = (date: Date | number) => timeFormatter.format(date)

export const formatDateTime = (iso: string) => dateTimeFormatter.format(new Date(iso))

export function formatDuration(milliseconds: number) {
  const seconds = milliseconds / 1000
  if (seconds < 60) return `${secondsFormatter.format(seconds)} s`

  const minutes = Math.floor(seconds / 60)
  if (minutes < 60) return `${minutes} min ${Math.floor(seconds % 60)} s`

  return `${Math.floor(minutes / 60)} h ${minutes % 60} min`
}
