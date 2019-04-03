import message from 'antd/lib/message'
import React, { useEffect } from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { authActions } from '../../data/auth'
import { connect, Store } from '../../store'

import { LoginForm } from './Form'
import styles from './index.module.scss'


type Props = Pick<Store, 'auth' | 'startup'> & RouteComponentProps

const log = console.log.bind(console, 'LoginPage')

export const LoginPage = connect<RouteComponentProps, Pick<Store, 'auth' | 'startup'>>(
  ({ auth, startup }) => ({ auth, startup }),
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

    const { auth } = props

    if (auth.access) return null

    return (
      <div className={styles.page}>
        <LoginForm onSubmit={handleLogin(auth)} />
      </div>
    )
  },
)

const handleLogin = (auth: Props['auth']) => (name: string, password: string) => authActions
  .login({ name, password })
  .catch(error => message.error(`login failed: ${error}`))

const ensureAccess = ({ auth, startup, history }: Props) => {
  log('checkAccess', auth.access)
  if ((auth.user.isSuccess || auth.user.isFailure) && auth.access)
    if (startup.location.pathname.startsWith('/login'))
      history.replace('/')
    else
      history.replace(startup.location)
}
