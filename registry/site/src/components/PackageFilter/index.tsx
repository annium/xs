import Input from 'antd/lib/input'
import React from 'react'

import { ProjectType } from '../../models/view/ProjectType'
import { ProjectTypeSelect } from '../ProjectTypeSelect'

import styles from './index.module.scss'

type Props = {
  type: ProjectType
  query: string
  onSubmit(): void;
  onTypeChange(type: ProjectType): void;
  onQueryChange(query: string): void;
}

export const PackageFilter = ({ type, onTypeChange, query, onQueryChange, onSubmit }: Props) => {
  const handleQueryChange = (e: React.ChangeEvent<HTMLInputElement>) => onQueryChange(e.target.value)

  return (
    <div className={styles.filter}>
      <ProjectTypeSelect type={type} onSelect={onTypeChange} />
      <Input.Search
        placeholder="search packages"
        enterButton={true}
        value={query}
        onChange={handleQueryChange}
        onSearch={onSubmit}
      />
    </div>
  )
}
