import { Response } from '@xs/site.lib/dist/api'

export type AsyncState<T> = {
  data: T
  error?: string
  isRunning: boolean
  isSuccess: boolean
  isFailure: boolean
}

export const create = <T>(data: T): AsyncState<T> => ({
  data,
  error: undefined,
  isRunning: false,
  isSuccess: false,
  isFailure: false,
})

export const load = <T>(state: AsyncState<T>): AsyncState<T> => {
  state.isRunning = true

  return state
}

export const complete = <T>(state: AsyncState<T>, response: Response<T> | Response<void>): AsyncState<T> => {
  state.isRunning = false
  if (response.data)
    state.data = response.data
  state.error = response.error
  state.isSuccess = response.isSuccess
  state.isFailure = response.isFailure

  return state
}
