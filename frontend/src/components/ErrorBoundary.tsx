import { Component, type ErrorInfo, type ReactNode } from 'react'

interface Props {
  children: ReactNode
}

interface State {
  error: Error | null
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null }

  static getDerivedStateFromError(error: Error): State {
    return { error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Unhandled render error', error, errorInfo)
  }

  render() {
    if (this.state.error) {
      return (
        <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-gray-50 px-4 text-center">
          <h1 className="text-xl font-semibold text-gray-900">Что-то пошло не так</h1>
          <p className="text-gray-500">Попробуйте перезагрузить страницу.</p>
          <button
            type="button"
            onClick={() => window.location.reload()}
            className="rounded bg-gray-900 px-4 py-2 text-sm font-medium text-white"
          >
            Перезагрузить
          </button>
        </div>
      )
    }

    return this.props.children
  }
}
