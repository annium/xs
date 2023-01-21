import { HttpResponse } from '@annium/client-http'

import { User } from '../models/view/User'

import { api } from './api'


export const login = (name: string, password: string): Promise<HttpResponse> =>
  api.post('login', undefined, { name, password })

export const load = (): Promise<HttpResponse<User>> =>
  api.get<User>('login')

export const logout = (): Promise<HttpResponse> =>
  api.delete('login')

export const update = (name: string, password: string): Promise<HttpResponse> =>
  api.post('user', undefined, { name, password })

export const updateToken = (): Promise<HttpResponse> =>
  api.post('user/token')
