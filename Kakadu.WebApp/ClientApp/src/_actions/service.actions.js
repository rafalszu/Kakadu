import { serviceConstants } from '../_constants';
import { serviceService } from '../_services';

export const serviceActions = {
    getAll
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