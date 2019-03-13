import message from 'antd/lib/message'
import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Store } from '../../store'

import { LoginForm } from './Form'
import styles from './index.module.scss'


type Props = Pick<Store, 'startup' | 'user'> & RouteComponentProps

const log = console.log.bind(console, 'LoginPage')
@inject((stores: Store) => ({
  startup: stores.startup,
  user: stores.user,
}))
@observer
export class LoginPage extends React.Component<Props> {
  public async componentWillMount() {
    log('componentWillMount', 'ensure access')
    this.ensureAccess()
  }

  public async componentDidUpdate() {
    log('componentDidUpdate', 'ensure access')
    this.ensureAccess()
  }

  public render() {
    const { user } = this.props

    if (user.hasAccess) return null

    const handleLogin = (name: string, password: string) => user
      .login(name, password)
      .catch(error => message.error(`login failed: ${error}`))

    return (
      <div className={styles.page}>
        <LoginForm onSubmit={handleLogin} />
      </div>
    )
  }

  private ensureAccess() {
    const { startup, user, history } = this.props
    log('checkAccess', user.hasAccess)
    if (user.isLoaded && user.hasAccess)
      if (startup.location.pathname.startsWith('/login'))
        history.replace('/')
      else
        history.replace(startup.location)
  }
}
