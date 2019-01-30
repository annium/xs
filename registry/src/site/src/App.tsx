import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import Loader from './components/Loader'
import { StartupStore } from './data/startup'
import { UserStore } from './data/user'
import { Store } from './store'


interface Props extends RouteComponentProps {
  startup: StartupStore
  user: UserStore
}

const log = console.log.bind(console, 'App')
@inject((stores: Store) => ({
  startup: stores.startup,
  user: stores.user,
}))
@observer
export default class App extends React.Component<Props> {
  async componentWillMount() {
    const { startup, user, location } = this.props
    startup.location = location
    log('componentWillMount', 'load user')
    await user.load()
  }

  render() {
    const { user, children } = this.props
    log('render')

    if (!user.isLoaded)
      return <Loader isLoading={!user.isLoaded} size="big" />

    return children
  }
}