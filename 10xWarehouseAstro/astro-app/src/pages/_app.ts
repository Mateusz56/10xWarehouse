import type { App } from 'vue'
import { pinia } from '@/stores'
import { clientRouter } from './_clientRouter'

export default (app: App) => {
  app.use(pinia)
  if (clientRouter) {
    app.use(clientRouter)
  }
}
