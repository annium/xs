import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { UserStore } from './data/user'
import { Store } from './store'


interface Props extends RouteComponentProps {
  user: UserStore
}

const log = console.log.bind(console, 'App')
@inject((stores: Store) => ({ user: stores.user }))
@observer
export default class App extends React.Component<Props> {
  async componentWillMount() {
    log('componentWillMount', 'load user')
    await this.props.user.load()
  }

  render() {
    log('render')

    return this.props.children
  }
}