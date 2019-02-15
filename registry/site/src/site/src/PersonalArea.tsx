import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import Root from './components/Root'
import { Store } from './store'


type Props = Pick<Store, 'startup' | 'user'> & RouteComponentProps

const log = console.log.bind(console, 'PersonalArea')
@inject((stores: Store) => ({
  startup: stores.startup,
  user: stores.user,
}))
@observer
export default class PersonalArea extends React.Component<Props> {
  async componentWillMount() {
    const { startup } = this.props

    log('componentWillMount', 'ensure access')
    this.ensureAccess()
    await startup.load()
  }

  async componentDidUpdate() {
    log('componentDidUpdate', 'ensure access')
    this.ensureAccess()
  }

  render() {
    const { user, children } = this.props
    if (!user.hasAccess)
      return null

    log('render')
    return (
      <Root>
        {children}
      </Root>
    )
  }

  private ensureAccess(): void {
    const { user, history } = this.props
    log('checkAccess', user.hasAccess)
    if (user.isLoaded && !user.hasAccess)
      history.replace('/login')
  }
}