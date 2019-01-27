import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import LoginForm from './Form'
import { Store } from '../../store'
import { UserStore } from '../../data/user'


import styles from './index.module.scss'


interface Props extends RouteComponentProps {
  user: UserStore
}

const log = console.log.bind(console, 'LoginPage')
@inject((stores: Store) => ({ user: stores.user }))
@observer
export default class LoginPage extends React.Component<Props> {
  async componentWillMount() {
    log('componentWillMount', 'ensure access')
    this.ensureAccess()
  }

  async componentDidUpdate() {
    log('componentDidUpdate', 'ensure access')
    this.ensureAccess()
  }

  render() {
    const { user } = this.props

    if (user.hasAccess) return null

    return (
      <div className={styles.page}>
        <LoginForm onSubmit={user.login} />
      </div>
    )
  }

  // renderOld() {
  //   const { user } = this.props

  //   if (user.hasAccess) return null
  //   log('render', user)

  //   return (
  //     <div className="login-page">
  //       <input placeholder="name" onChange={this.setName} />
  //       <input placeholder="pass" onChange={this.setPassword} />
  //       <button onClick={() => user.login(name, password)}>login</button>
  //     </div >
  //   )
  // }

  // private setName = (e: React.ChangeEvent<HTMLInputElement>) => {
  //   this.name = e.target.value
  // }

  // private setPassword = (e: React.ChangeEvent<HTMLInputElement>) => {
  //   this.password = e.target.value
  // }

  private ensureAccess() {
    const { user, history } = this.props
    log('checkAccess', user.hasAccess)
    if (user.hasAccess)
      history.replace('/')
  }
}