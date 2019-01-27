import * as React from 'react'

import Menu from './Menu'


import styles from './index.module.scss'


export default class Root extends React.Component {
  render() {
    return (
      <div className={styles.root}>
        <div className={styles.content}>
          {this.props.children}
        </div>
        <Menu />
      </div>
    )
  }
}