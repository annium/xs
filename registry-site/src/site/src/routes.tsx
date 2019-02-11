import createBrowserHistory from 'history/createBrowserHistory'
import * as React from 'react'
import { Route, Router, Switch } from 'react-router-dom'

import App from './App'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import PackagePage from './pages/PackagePage'
import PackagesPage from './pages/PackagesPage'
import SettingsPage from './pages/SettingsPage'
import PersonalArea from './PersonalArea'

const navHistory = createBrowserHistory()


const Routes: React.SFC = () => (
  <Router history={navHistory}>
    <Route render={renderApp} />
  </Router>
)

const renderApp = (props: any) => (
  <App {...props}>
    <Switch>
      <Route path="/login" exact component={LoginPage} />
      <Route render={renderPersonalArea} />
    </Switch>
  </App>
)

const renderPersonalArea = (props: any) => (
  <PersonalArea {...props}>
    <Switch>
      <Route path="/" exact component={HomePage} />
      <Route path="/packages/:type/:name(.*)" component={PackagePage} />
      <Route path="/packages" exact component={PackagesPage} />
      <Route path="/settings" exact component={SettingsPage} />
      <Route render={() => <h1>Not found</h1>} />
    </Switch>
  </PersonalArea>
)

export default Routes