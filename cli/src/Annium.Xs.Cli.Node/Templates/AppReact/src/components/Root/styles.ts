import { makeStyles } from '@material-ui/core/styles'

import { flex } from '../../styles/mixins'

export const useStyles = makeStyles(() => ({
    root: {
        ...flex('column'),
        flex: 1,
    },
}))
