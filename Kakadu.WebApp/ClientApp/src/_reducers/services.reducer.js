import { serviceConstants, captureConstants } from '../_constants';

export function services(state = {services: []}, action) {
  switch (action.type) {
    case serviceConstants.GETALL_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.GETALL_SUCCESS:
      return {
        items: action.services
      };
    case serviceConstants.GETALL_FAILURE:
      return { 
        error: action.error
      };
    case serviceConstants.GETBYCODE_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.GETBYCODE_SUCCESS:
      return {
        item: action.service
      };
    case serviceConstants.GETBYCODE_FAILURE:
      return {
        error: action.error
      };
    case serviceConstants.UPDATE_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.UPDATE_SUCCESS: 
      return {
        item: action.service
      };
    case serviceConstants.UPDATE_FAILURE:
      return {
        error: action.error
      };
    case serviceConstants.CREATE_REQUEST:
      return {
        loading: true
      };
    case serviceConstants.CREATE_SUCCESS:
      return {
        item: action.service
      };
    case serviceConstants.CREATE_FAILURE:
      return {
        error: action.error
      };
      case serviceConstants.REMOVE_REQUEST:
        return {
          ...state,
          items: state.items.map(service =>
            service.code === action.serviceCode
              ? { ...service, deleting: true }
              : service
          )
        };
      case serviceConstants.REMOVE_SUCCESS:
        return {
          items: state.items.filter(service => service.code !== action.serviceCode)
        };
      case serviceConstants.REMOVE_FAILURE:
        return {
          ...state,
          items: state.items.map(service => {
            if (service.code === action.serviceCode) {
              const { deleting, ...serviceCopy } = service;
              return { ...serviceCopy, deleteError: action.error };
            }

            return service;
          })
        };
        case captureConstants.START_REQUEST:
        case captureConstants.STOP_REQUEST:
          return {
              ...state,
              items: state.items.map(service =>
                service.code === action.serviceCode
                  ? { ...service, awaitingActionResult: true }
                  : service
              )
            };
        case captureConstants.START_SUCCESS:
          return {
              ...state,
              items: state.items.map(service =>
                  service.code === action.serviceCode
                  ? { ...service, isRecording: true, awaitingActionResult: false }
                  : service
              )
          };
        case captureConstants.START_FAILURE:
        case captureConstants.STOP_FAILURE:
          return {
              ...state,
              items: state.items.map(service => {
                if (service.code === action.serviceCode) {
                  const { awaitingActionResult, ...serviceCopy } = service;
                  return { ...serviceCopy, error: action.error };
                }
    
                return service;
              })
            };
        case captureConstants.STOP_SUCCESS:
          return {
              ...state,
              items: state.items.map(service =>
                  service.code === action.serviceCode
                  ? { ...service, isRecording: false, awaitingActionResult: false }
                  : service
              )
          };
    default:
      return state
  }
}