import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import { Loader } from './components/Loader'
import { Store } from './store'


type Props = Pick<Store, 'startup' | 'user'> & RouteComponentProps

const log = console.log.bind(console, 'App')
@inject((stores: Store) => ({
  startup: stores.startup,
  user: stores.user,
}))
@observer
export class App extends React.Component<Props> {
  public async componentWillMount() {
    const { startup, user, location } = this.props
    startup.location = location
    log('componentWillMount', 'load user')
    await user.load()
  }

  public render() {
    const { user, children } = this.props
    log('render')

    if (!user.isLoaded)
      return <Loader isLoading={!user.isLoaded} size="big" />

    return children
  }
}
