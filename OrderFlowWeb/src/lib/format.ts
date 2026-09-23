const currencyFormatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' })

const timeFormatter = new Intl.DateTimeFormat('pt-BR', { timeStyle: 'medium' })

export const formatCurrency = (value: number) => currencyFormatter.format(value)

export const formatTime = (date: Date | number) => timeFormatter.format(date)

export const formatDateTime = (iso: string) => dateTimeFormatter.format(new Date(iso))
