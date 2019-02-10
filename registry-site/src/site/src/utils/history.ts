import { History } from 'history'

export const updateLocation = (history: History, parameters: { [key: string]: string }) => {
  const { location } = history
  const params = new URLSearchParams(location.search)
  for (const param in parameters)
    if (parameters[param])
      params.set(param, parameters[param])
    else
      params.delete(param)

  history.replace({ ...location, search: params.toString() })
}