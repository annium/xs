import message from 'antd/lib/message'
import React, { useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { inject, Store } from '../../store'

import { LoginForm } from './Form'
import styles from './index.module.scss'


type Props = Pick<Store, 'startup' | 'user'> & RouteComponentProps

const log = console.log.bind(console, 'LoginPage')

export const LoginPage = inject<RouteComponentProps, Pick<Store, 'startup' | 'user'>>(
  ({ startup, user }) => ({ startup, user }),
  (props: Props) => {
    useEffect(
      () => {
        log('mount', 'ensure access')
        ensureAccess(props)
      },
      [],
    )

    useEffect(() => {
      log('update', 'ensure access')
      ensureAccess(props)
    })

    const { user } = props

    if (user.hasAccess) return null

    return (
      <div className={styles.page}>
        <LoginForm onSubmit={handleLogin(user)} />
      </div>
    )
  },
)

const handleLogin = (user: Props['user']) => (name: string, password: string) => user
  .login(name, password)
  .catch(error => message.error(`login failed: ${error}`))

const ensureAccess = ({ startup, user, history }: Props) => {
  log('checkAccess', user.hasAccess)
  if (user.isLoaded && user.hasAccess)
    if (startup.location.pathname.startsWith('/login'))
      history.replace('/')
    else
      history.replace(startup.location)
}
