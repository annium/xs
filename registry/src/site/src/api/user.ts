import { Response } from 'site.lib/dist/api'

import api from './api'

import User from '../models/view/User'

export default {
  login(name: string, password: string): Promise<Response> {
    return api.post('login', undefined, { name, password })
  },
  load(): Promise<Response<User | null>> {
    return api.get<User>('login')
  },
  logout(): Promise<Response> {
    return api.delete('login')
  },
  update(name: string, password: string): Promise<Response> {
    return api.post('user', undefined, { name, password })
  },
  updateToken(): Promise<Response<string>> {
    return api.post('user/token')
  },
}