// <input type="date"> yields "yyyy-MM-dd" in the admin's local timezone. The API compares against
// UTC timestamps, so a picked day is converted to the [start, next start) range of that *local* day.

export function startOfLocalDayIso(day: string): string {
  return new Date(`${day}T00:00:00`).toISOString()
}

export function startOfNextLocalDayIso(day: string): string {
  const start = new Date(`${day}T00:00:00`)
  start.setDate(start.getDate() + 1)
  return start.toISOString()
}
