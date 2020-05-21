import { serviceConstants } from '../_constants';
import { serviceService } from '../_services';

export const serviceActions = {
    getAll,
    getById
};

function getAll() {
    return dispatch => {
        dispatch(request());

        serviceService.getAll()
            .then(
                services => dispatch(success(services)),
                error => dispatch(failure(error && error.message))
            );
    };

    function request() { return { type: serviceConstants.GETALL_REQUEST } }
    function success(services) { return { type: serviceConstants.GETALL_SUCCESS, services } }
    function failure(error) { return { type: serviceConstants.GETALL_FAILURE, error } }
}

function getById(id) {
    return dispatch => {
        dispatch(request());

        serviceService.getById(id)
            .then(
                service => dispatch(success(service)),
                error => dispatch(failure(error && error.message))
            );
    };

    function request() { return { type: serviceConstants.GETBYID_REQUEST } }
    function success(service) { return { type: serviceConstants.GETBYID_SUCCESS, service } }
    function failure(error) { return { type: serviceConstants.GETBYID_FAILURE, error } }
}