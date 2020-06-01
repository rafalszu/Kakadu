import { authHeader, apiUrl, handleResponse } from '../_helpers';

export const serviceService = {
    getAll,
    getByCode,
    update,
    create,
    remove
};

const requestOptions = {
    method: 'GET',
    headers: authHeader()
};

async function getAll() {
    const response = await fetch(`${apiUrl}/service`, requestOptions);
    return handleResponse(response);
}

async function getByCode(code) {
    const response = await fetch(`${apiUrl}/service/${code}`, requestOptions);
    return handleResponse(response);
}

async function update(serviceCode, data) {
    const response = await fetch(`${apiUrl}/service/${serviceCode}`, {
        method: 'PATCH',
        headers: Object.assign(authHeader(), {
            'Content-Type': 'application/json',
        }),
        body: JSON.stringify(data)
    });

    return handleResponse(response);
}

async function create(service) {
    const response = await fetch(`${apiUrl}/service`, {
        method: 'POST',
        headers: Object.assign(authHeader(), {
            'Content-Type': 'application/json',
        }),
        body: JSON.stringify(service)
    });

    return handleResponse(response);
}

async function remove(serviceCode) {
    const response = await fetch(`${apiUrl}/service/${serviceCode}`, {
        method: 'DELETE',
        headers: authHeader(),
    });

    return handleResponse(response);
}
