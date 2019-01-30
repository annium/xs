import { inject, observer } from 'mobx-react'
import * as React from 'react'
import { RouteComponentProps } from 'react-router-dom'

import Root from './components/Root'
import { UserStore } from './data/user'
import { Store } from './store'


interface Props extends RouteComponentProps {
  user: UserStore
}

const log = console.log.bind(console, 'PersonalArea')
@inject((stores: Store) => ({ user: stores.user }))
@observer
export default class PersonalArea extends React.Component<Props> {
  async componentWillMount() {
    log('componentWillMount', 'ensure access')
    this.ensureAccess()
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