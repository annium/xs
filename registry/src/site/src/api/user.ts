import { Response } from 'site.lib/dist/api'

import api from './api'

import User from '../models/view/User'

export default {
  login(login: string, password: string): Promise<Response> {
    return api.post('/login', undefined, { login, password })
  },
  load(): Promise<Response<User | null>> {
    return api.get<User>('/login')
  },
  logout(): Promise<Response> {
    return api.delete('/login')
  },
}