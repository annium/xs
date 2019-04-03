import { Response } from '@annium/server-http'

import { User } from '../models/view/User'

import { api } from './api'


export const login = (name: string, password: string): Promise<Response> =>
  api.post('login', undefined, { name, password })

export const load = (): Promise<Response<User>> =>
  api.get<User>('login')

export const logout = (): Promise<Response> =>
  api.delete('login')

export const update = (name: string, password: string): Promise<Response> =>
  api.post('user', undefined, { name, password })

export const updateToken = (): Promise<Response> =>
  api.post('user/token')
