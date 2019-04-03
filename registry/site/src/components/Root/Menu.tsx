import Icon from 'antd/lib/icon'
import React from 'react'
import { NavLink, RouteComponentProps, withRouter } from 'react-router-dom'

import { authActions } from '../../data/auth'
import { connect, Store } from '../../store'


import styles from './Menu.module.scss'

type SelectorProps = Partial<Pick<Store, 'auth'>>
type Props = SelectorProps & RouteComponentProps

export const Menu = withRouter(connect<RouteComponentProps, SelectorProps>(
  ({ auth }) => ({ auth }),
  ({ auth }: Props) => {
    const { user } = auth!

    return (
      <div className={styles.menu}>
        <NavLink className={styles.item} exact={true} activeClassName={styles.isActiveItem} to="/">
          <Icon type="home" /> Home
        </NavLink>
        <NavLink className={styles.item} exact={true} activeClassName={styles.isActiveItem} to="/packages">
          <Icon type="search" /> Packages
        </NavLink>
        <div className={styles.separator} />
        <div className={styles.info}>Hi, {user.data ? user.data.name : ''}</div>
        <div className={styles.separator} />
        <NavLink className={styles.item} exact={true} activeClassName={styles.isActiveItem} to="/settings">
          <Icon type="setting" /> Settings
        </NavLink>
        <NavLink className={styles.item} to="/login" onClick={authActions.logout}>
          <Icon type="logout" /> Log out
        </NavLink>
      </div>
    )
  },
))
